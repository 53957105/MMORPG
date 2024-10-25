using MMORPG.Common.Network;
using MMORPG.Common.Proto;
using GameServer.Db;
using GameServer.Manager;
using GameServer.Network;
using GameServer.NetService;
using GameServer.Tool;
using Serilog;
using Serilog.Events;

// 命名空间GameServer包含程序的主要入口点和相关类
namespace GameServer
{
    // Program类包含程序的入口方法
    internal class Program
    {
        /// <summary>
        /// 程序的入口方法
        /// 初始化日志配置并启动游戏服务器
        /// </summary>
        /// <param name="args">命令行参数</param>
        static async Task Main(string[] args)
        {
            // 配置日志记录，包括最小级别、异步写入控制台和每日滚动文件
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Async(a => a.Console())
                .WriteTo.Async(a => a.File("Logs/log-.txt", rollingInterval: RollingInterval.Day))
                .CreateLogger();

            // 以下代码行被注释掉，用于创建和插入数据库角色
            // var character = new DbCharacter("jj", 1, 1, 1, 1, 1, 1, 1, 1);
            // SqlDb.Connection.Insert(character).ExecuteAffrows();

            // 创建并运行游戏服务器实例
            GameServer server = new(NetConfig.ServerPort);
            await server.Run();
        }
    }
}
