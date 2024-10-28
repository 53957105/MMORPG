using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG.Common.Tool
{
    public class TimeWheel
    {
        private const int CircleCount = 5;
        private const int SlotCount = 1 << 6;

        public struct TimeTask
        {
            public int Tick;
            public Action<TimeTask> Action;
            public LinkedListNode<TimeTask> LinkedListNode;
        };

        private LinkedList<TimeTask> _addList;
        private LinkedList<TimeTask> _backupAddList;
        private List<TimeTask> _removeList;
        private List<TimeTask> _backupRemoveList;
        private LinkedList<TimeTask>[] _slot;
        private int[] _indexArr;

        private long _lastMs;
        private int _tickMs;  // 最小槽的时间范围，毫秒单位

        private bool _stop;

        /// <summary>
        /// 初始化TimeWheel类的新实例。
        /// </summary>
        /// <param name="tickMs">可选参数，表示每个刻度的时间间隔（以毫秒为单位）。默认值为10毫秒。</param>
        public TimeWheel(int tickMs = 10) {
            // 初始化添加列表
            _addList = new();
            // 初始化备份添加列表
            _backupAddList = new();
            // 初始化移除列表
            _removeList = new();
            // 初始化备份移除列表
            _backupRemoveList = new();
            // 初始化时隙数组，用于存储时间任务
            _slot = new LinkedList<TimeTask>[SlotCount * CircleCount];
            // 初始化索引数组，用于跟踪每个圆的当前时隙
            _indexArr = new int[CircleCount];
            // 初始化刻度时间间隔
            _tickMs = tickMs;
            // 初始化停止标志为false
            _stop = false;
        
            // 初始化所有时隙
            for (int i = 0; i < SlotCount * CircleCount; i++)
            {
                _slot[i] = new();
            }
            // 初始化所有索引为0
            for (int i = 0; i < CircleCount; i++)
            {
                _indexArr[i] = 0;
            }
        }

        public async Task Start()
        {
            _lastMs = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond / _tickMs;
            do
            {
                if (_addList.Count > 0)
                {
                    lock (_addList)
                    {
                        (_backupAddList, _addList) = (_addList, _backupAddList);
                    }
                    DispatchTasksToSlot(_backupAddList);
                }
                if (_removeList.Count > 0)
                {
                    lock (_removeList)
                    {
                        (_backupRemoveList, _removeList) = (_removeList, _backupRemoveList);
                    }
                    for (int i = 0; i < _backupRemoveList.Count; i++)
                    {
                        var node = _backupRemoveList[i].LinkedListNode;
                        var list = node.List;
                        list?.Remove(node);
                    }
                    _backupRemoveList.Clear();
                }

                // 根据上次循环至今经过的时间，逐格推进
                var nowMs = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond / _tickMs;
                var duration = nowMs - _lastMs;
                for (int i = 0; i < duration; i++)
                {
                    if (_slot[_indexArr[0]].Count > 0)
                    {
                        foreach (var task in _slot[_indexArr[0]])
                        {
                            try
                            {
                                task.Action(task);
                            }
                            catch
                            {
                                // ignored
                            }
                            // Task.Cast(task.Action);
                        }
                        _slot[_indexArr[0]].Clear();
                    }

                    // 如果需要向上层时间轮推进，则将上层时间轮的队列向下派发
                    int j = 0;
                    ++_indexArr[0];
                    do
                    {
                        bool ge = _indexArr[j] >= SlotCount;
                        if (ge) _indexArr[j] = 0;
                        if (j > 0)
                        {
                            int index = j * SlotCount + _indexArr[j];
                            DispatchTasksToSlot(_slot[index]);
                        }
                        if (!ge || ++j >= CircleCount) break;
                        ++_indexArr[j];
                    } while (true);
                }
                _lastMs = nowMs;
                await Task.Delay(_tickMs);
            } while (_stop == false);
        }

        void Stop()
        {
            _stop = true;
        }

        private int GetLayerByTick(int tick)
        {
            const int mask = 0x3f; // 0011 1111
            for (int i = 0; i < CircleCount; i++)
            {
                if ((tick & ~mask) == 0)
                {
                    return i;
                }
                tick >>= 6;
            }
            throw new Exception("TimeWheel.GetLayerByTick: Tick too large.");
        }

        private void DispatchTasksToSlot(LinkedList<TimeTask> list)
        {
            for (var task = list.First; task != null; )
            {
                var next = task.Next;

                // 根据时长插入对应的槽中
                int tick = task.Value.Tick;

                int layer = GetLayerByTick(tick);
                int index = tick & 0x3f;
                index = layer * SlotCount + ((index + _indexArr[layer]) % SlotCount);

                // 清除在当前层的时长，在下次向下派发时可以插入到下层
                int mask2 = ~(0x7fffffff << (layer * 6));
                task.ValueRef.Tick &= mask2;

                list.Remove(task);
                _slot[index].AddLast(task);

                task = next;
            }
        }

        /// <summary>
        /// 异步追加延时任务到时间轮中
        /// 不可修改返回的task
        /// </summary>
        public TimeTask AddTask(int ms, Action<TimeTask> action)
        {
            if (ms < _tickMs) {
                ms = _tickMs;
            }
            var task = new TimeTask()
            {
                Tick = ms / _tickMs,
                Action = action,
            };
            lock (_addList)
            {
                var node = _addList.AddLast(task);
                task.LinkedListNode = node;
            }
            return task;
        }

        /// <summary>
        /// 异步删除延时任务
        /// </summary>
        /// <param name="task"></param>
        public void RemoveTask(TimeTask task)
        {
            lock (_removeList)
            {
                _removeList.Add(task);
            }
        }
    }

    public class TimeWheelTest
    {
        public static async Task Start()
        {
            //Console.WriteLine($"start:{DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond}");
            int count = 0;
            var tw = new TimeWheel(1);
            tw.Start();
            var task = tw.AddTask(1, (task) => {
                Console.WriteLine($"hello");
            });
            await Task.Delay(1000);
            tw.RemoveTask(task);


            var begin = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
           
            var randomNumber = new byte[4];
            for (int i = 0; i < 1000000; i++)
            {
                //using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                //{
                //    rng.GetBytes(randomNumber);
                //}
                //int randomValue = BitConverter.ToInt32(randomNumber, 0);
                //randomValue = Math.Abs(randomValue);
                //randomValue %= 10;

                int j = i;
                tw.AddTask(1, (task) => {
                    //Console.WriteLine($"[{j}][{count++}]{randomValue}ms:{DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond}");
                    //count++;
                    Interlocked.Increment(ref count);
                });
            }
            while (count < 1000000)
            {
                await Task.Delay(1);
            }
            var end = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            //await Task.Delay(1000000);
            Console.WriteLine($"end:{end - begin}ms");
        }
    }

}
