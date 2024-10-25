using MMORPG.Common.Network;
using GameServer.Network;
using GameServer.NetService;
using Service;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using MMORPG.Common.Tool;
using GameServer.Manager;
using Serilog;

namespace GameServer
{
    public class GameServer
    {
        // 用于监听和接受客户端连接的服务器套接字
        private Socket _serverSocket;
        
        // 存储所有当前活跃的网络通道的链表
        private LinkedList<NetChannel> _channels;
        
        // 用于连接清理的定时器，定期检查并关闭不活跃的连接
        private TimeWheel _connectionCleanupTimer;

        /// <summary>
        /// 初始化GameServer类的新实例。
        /// </summary>
        /// <param name="port">服务器将要监听的端口号。</param>
        public GameServer(int port)
        {
            // 创建一个服务器套接字，使用IPv4地址族，流式套接字类型和TCP协议
            _serverSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            // 绑定服务器套接字到指定端口和任何IP地址（0.0.0.0表示监听所有可用的网络接口）
            _serverSocket.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), port));
            
            // 初始化频道集合，用于管理客户端连接
            _channels = new();
            
            // 创建一个定时器，用于定期清理断开的连接，初始设置为每秒触发一次
            _connectionCleanupTimer = new(1000);
        }

        /// <summary>
        /// 启动服务器并处理客户端连接
        /// </summary>
        public async Task Run()
        {
            // 记录服务器启动信息
            Log.Information("[Server] 开启服务器");
        
            // 开始初始化Manager
            Log.Information("[Server] 开始初始化Manager...");
            UpdateManager.Instance.Start();
            // 记录Manager初始化完成信息
            Log.Information("[Server] Manager初始化完成");
        
            // 开始监听客户端连接
            _serverSocket.Listen();
            // 启动连接清理定时器（已注释）
            //_connectionCleanupTimer.Start();
            
            // 无限循环以持续接受新的客户端连接
            while (true)
            {
                // 异步接受客户端连接
                var socket = await _serverSocket.AcceptAsync();
                // 记录客户端连接信息
                Log.Information($"[Server] 客户端连接:{socket.RemoteEndPoint}");
                
                // 创建新的NetChannel对象以处理客户端连接
                NetChannel channel = new(socket);
                // 触发新连接的处理事件
                OnNewChannelConnection(channel);
                
                // 在新的任务中启动NetChannel以处理客户端通信
                Task.Run(channel.StartAsync);
            }
        }

        /// <summary>
        /// 处理新通道连接的事件
        /// </summary>
        /// <param name="sender">建立连接的NetChannel对象</param>
        private void OnNewChannelConnection(NetChannel sender)
        {
            // 锁定_channels以防止多线程访问冲突
            lock (_channels)
            {
                // 将新的NetChannel添加到链表中并更新其LinkedListNode属性
                var node = _channels.AddLast(sender);
                sender.LinkedListNode = node;
            }
        
            // 为新连接的NetChannel注册事件处理程序
            sender.PacketReceived += OnPacketReceived;
            sender.ConnectionClosed += OnConnectionClosed;
            // 更新最后一次活跃时间以初始化连接清理计时器
            sender.LastActiveTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
        
            // 添加一个任务到连接清理计时器以在指定时间后检查连接的活动状态
            _connectionCleanupTimer.AddTask(ChannelConfig.CleanupMs, (task) =>
            {
                // 计算当前时间和连接最后一次活跃时间之间的差异
                var now = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond; ;
                var duration = now - sender.LastActiveTime;
                // 如果连接在指定时间内没有活动，则关闭它
                if (duration > ChannelConfig.CleanupMs)
                {
                    // sender已关闭也不会产生错误
                    sender.Close();
                }
                else
                {
                    // 如果连接仍然活跃，重新安排任务以稍后再次检查
                    _connectionCleanupTimer.AddTask(ChannelConfig.CleanupMs, task.Action);
                }
            });
        
            // 通知各个服务新连接的NetChannel
            UserService.Instance.OnConnect(sender);
            CharacterService.Instance.OnConnect(sender);
            MapService.Instance.OnConnect(sender);
            PlayerService.Instance.OnConnect(sender);
            NpcService.Instance.OnConnect(sender);
        }

        /// <summary>
        /// 当网络连接关闭时调用此方法
        /// </summary>
        /// <param name="sender">触发事件的源，此处为NetChannel对象</param>
        /// <param name="e">包含事件数据的参数</param>
        private void OnConnectionClosed(object? sender, ConnectionClosedEventArgs e)
        {
            // 将sender转换为NetChannel类型，以便进行后续操作
            var channel = sender as NetChannel;
            // 断言channel不为null，确保类型转换成功
            Debug.Assert(channel != null);
        
            // 调用UserService的实例方法，处理通道关闭事件
            UserService.Instance.OnChannelClosed(channel);
            // 调用CharacterService的实例方法，处理通道关闭事件
            CharacterService.Instance.OnChannelClosed(channel);
            // 调用MapService的实例方法，处理通道关闭事件
            MapService.Instance.OnChannelClosed(channel);
            // 调用PlayerService的实例方法，处理通道关闭事件
            PlayerService.Instance.OnChannelClosed(channel);
            // 调用NpcService的实例方法，处理通道关闭事件
            NpcService.Instance.OnChannelClosed(channel);
        
            // 锁定_channels集合，以防止多线程访问冲突
            lock (_channels)
            {
                // 检查channel是否在链表中
                if (channel.LinkedListNode != null)
                {
                    try
                    {
                        // 从链表中移除当前channel节点
                        _channels.Remove(channel.LinkedListNode);
                    }
                    catch (Exception exception)
                    {
                        //TODO _channels.Remove的报错处理
                        // 捕获移除操作中可能发生的异常，进行后续处理
                    }
                }
            }
        }

        /// <summary>
        /// 处理接收到数据包的事件。
        /// </summary>
        /// <param name="sender">事件发送者，预期为一个<see cref="NetChannel"/>对象。</param>
        /// <param name="e">事件参数，包含接收到的数据包信息。</param>
        private void OnPacketReceived(object? sender, PacketReceivedEventArgs e)
        {
            // 将事件发送者转换为NetChannel类型，以便进行网络通信相关的操作。
            var channel = sender as NetChannel;
            // 断言channel不为null，如果为null则表示转换失败，此处不应继续执行。
            Debug.Assert(channel != null);
        
            // 更新通道的最后活跃时间，以当前UTC时间为准，精确到毫秒。
            channel.LastActiveTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
        
            // 调用各个服务的HandleMessage方法处理接收到的消息，根据消息类型执行相应的逻辑。
            UserService.Instance.HandleMessage(channel, e.Packet.Message);
            CharacterService.Instance.HandleMessage(channel, e.Packet.Message);
            MapService.Instance.HandleMessage(channel, e.Packet.Message);
            PlayerService.Instance.HandleMessage(channel, e.Packet.Message);
            FightService.Instance.HandleMessage(channel, e.Packet.Message);
            InventoryService.Instance.HandleMessage(channel, e.Packet.Message);
            NpcService.Instance.HandleMessage(channel, e.Packet.Message);
        }
    }
}
