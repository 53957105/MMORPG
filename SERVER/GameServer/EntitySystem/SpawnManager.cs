using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GameServer.Tool;
using Serilog;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using System.Data;
using GameServer.MapSystem;
using GameServer.Manager;
using MMORPG.Common.Tool;

namespace GameServer.EntitySystem
{
    /// <summary>
    /// 实体生成器
    /// </summary>
    public class Spawner
    {
        public SpawnManager SpawnManager;
        public SpawnDefine SpawnDefine;
        public Actor? Actor;

        private bool _reviving;      // 复活中
        private float _reviveTime;   // 复活时间

        public Spawner(SpawnManager manager, SpawnDefine define)
        {
            SpawnManager = manager;
            SpawnDefine = define;
        }

        /// <summary>
        /// 初始化并生成游戏中的角色或NPC。
        /// </summary>
        public void Start()
        {
            // 解析角色或NPC的初始位置
            var pos = DataHelper.ParseVector3(SpawnDefine.Pos);
            // 解析角色或NPC的初始方向
            var dire = DataHelper.ParseVector3(SpawnDefine.Dir);
        
            // 获取指定ID的角色或NPC定义信息
            var unitDefine = DataManager.Instance.UnitDict[SpawnDefine.UnitID];
        
            // 根据定义信息中的种类，决定生成怪物还是NPC
            if (unitDefine.Kind == "Monster")
            {
                // 生成新的怪物实例
                Actor = SpawnManager.Map.MonsterManager.NewMonster(SpawnDefine, SpawnDefine.UnitID, pos, dire, unitDefine.Name, SpawnDefine.Level);
            }
            else if (unitDefine.Kind == "Npc")
            {
                // 生成新的NPC实例
                Actor = SpawnManager.Map.NpcManager.NewNpc(SpawnDefine.UnitID, pos, dire, unitDefine.Name, SpawnDefine.Level);
            }
        }

        public void Update()
        {
            if (Actor == null || !Actor.IsDeath()) return;
            if (!_reviving)
            {
                _reviveTime = Time.time + SpawnDefine.Period;
                _reviving = true;
            }
            if (Time.time >= _reviveTime)
            {
                Actor.Revive();
                _reviving = false;
            }
        }
    }


    /// <summary>
    /// 实体生成管理器
    /// </summary>
    public class SpawnManager
    {
        public Map Map;
        private List<Spawner> _spawners = new();

        public SpawnManager(Map map)
        {
            Map = map;
        }

        /// <summary>
        /// 启动方法，用于初始化和启动所有符合条件的生成器
        /// </summary>
        public void Start()
        {
            // 根据当前地图ID筛选出所有适用的规则
            var rules = DataManager.Instance.SpawnDict.Values.Where(r => r.MapId == Map.MapId);
        
            // 遍历每个筛选后的规则，创建并启动对应的生成器
            foreach (var rule in rules)
            {
                // 创建一个新的生成器实例，传入当前实例和规则
                var spawner = new Spawner(this, rule);
        
                // 将创建的生成器添加到生成器列表中
                _spawners.Add(spawner);
        
                // 启动生成器
                spawner.Start();
            }
        }

        public void Update()
        {
            foreach (var spawner in _spawners)
            {
                spawner.Update();
            }
        }
    }
}
