using MMORPG.Common.Tool;
using GameServer.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using System.Data;
using GameServer.Manager;

namespace GameServer.MapSystem
{

    /// <summary>
    /// 地图管理器
    /// 负责管理游戏的所有地图
    /// </summary>
    public class MapManager : Singleton<MapManager>
    {
        // 初始化地图ID，用于标识初始地图
        public readonly int InitMapId = 1;
        
        // 地图字典，用于存储和快速检索地图对象
        private Dictionary<int, Map> _mapDict = new();

        MapManager() { }

        /// <summary>
        /// 加载并初始化所有地图数据
        /// </summary>
        public void Start()
        {
            // 遍历地图字典，加载每张地图
            foreach (var mapDefine in DataManager.Instance.MapDict.Values)
            {
                // 记录日志：加载地图
                Log.Information($"加载地图：{mapDefine.Name}");
                // 调用新地图方法进行地图初始化
                NewMap(mapDefine);
            }
        }

        public void Update()
        {
            foreach (var map in _mapDict.Values)
            {
                map.Update();
            }
        }

        /// <summary>
        /// 创建并初始化一个新的地图实例。
        /// </summary>
        /// <param name="mapDefine">地图的定义信息，包含地图的ID和其他配置。</param>
        /// <returns>返回创建的地图实例。</returns>
        private Map NewMap(MapDefine mapDefine)
        {
            // 根据地图定义信息创建一个新的地图实例
            var map = new Map(mapDefine);
            
            // 将新创建的地图实例添加到地图字典中，以便后续检索和管理
            _mapDict.Add(mapDefine.ID, map);
            
            // 启动地图实例，进行必要的初始化工作
            map.Start();
            
            // 返回创建并初始化后的地图实例
            return map;
        }

        public Map? GetMapById(int mapId)
        {
            _mapDict.TryGetValue(mapId, out var map);
            return map;
        }

        public Map? GetMapByName(string mapName)
        {
            foreach (var map in _mapDict)
            {
                if (map.Value.Define.Name == mapName)
                {
                    return map.Value;
                }
            }
            return null;
        }
    }
}
