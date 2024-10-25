using GameServer.EntitySystem;
using GameServer.MapSystem;
using GameServer.System;
using GameServer.Tool;
using GameServer.UserSystem;
using MMORPG.Common.Proto.Fight;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Manager
{
    /// <summary>
    /// 负责所有组件的更新
    /// </summary>
    public class UpdateManager : Singleton<UpdateManager>
    {
        // 定义一个只读字段Fps，用于表示每秒的帧数
        public readonly int Fps = 10;
        
        // 创建一个队列，用于存储待执行的任务
        private Queue<Action> _taskQueue = new();
        
        // 创建一个备用队列，用于在主队列满或其他特殊情况时存储任务
        private Queue<Action> _backupTaskQueue = new();

        private UpdateManager()
        {
        }

        /// <summary>
        /// 启动服务器各个管理器的初始化和启动过程
        /// </summary>
        public void Start()
        {
            // 启动DataManager实例的初始化
            DataManager.Instance.Start();
            Log.Information("[Server] DataManager初始化完成");
        
            // 启动EntityManager实例的初始化
            EntityManager.Instance.Start();
            Log.Information("[Server] EntityManager初始化完成");
        
            // 启动MapManager实例的初始化
            MapManager.Instance.Start();
            Log.Information("[Server] MapManager初始化完成");
        
            // 启动UserManager实例的初始化
            UserManager.Instance.Start();
            Log.Information("[Server] UserManager初始化完成");
        
            // 在调度器中注册一个定时任务，每帧调用Update方法
            Scheduler.Instance.Register(1000 / Fps, Update);
        }

        public void Update()
        {
            Time.Tick();

            lock (_taskQueue)
            {
                (_backupTaskQueue, _taskQueue) = (_taskQueue, _backupTaskQueue);
            }

            foreach (var task in _backupTaskQueue)
            {
                try
                {
                    task();
                }
                catch (Exception e)
                {
                    Log.Error(e, "[UpdateManager] task()时出现报错");
                }
            }
            _backupTaskQueue.Clear();

            try
            {
                DataManager.Instance.Update();
            }
            catch (Exception e)
            {
                Log.Error(e, "[UpdateManager] DataManager.Instance.Update()时出现报错");
            }

            try
            {
                EntityManager.Instance.Update();
            }
            catch (Exception e)
            {
                Log.Error(e, "[UpdateManager] EntityManager.Instance.Update()时出现报错");
            }

            try
            {
                MapManager.Instance.Update();
            }
            catch (Exception e)
            {
                Log.Error(e, "[UpdateManager] MapManager.Instance.Update()时出现报错");
            }

            try
            {
                UserManager.Instance.Update();
            }
            catch (Exception e)
            {
                Log.Error(e, "[UpdateManager] UserManager.Instance.Update()时出现报错");
            }
        }

        /// <summary>
        /// 线程安全
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(Action task)
        {
            lock (_taskQueue)
            {
                _taskQueue.Enqueue(task);
            }
        }
    }
}
