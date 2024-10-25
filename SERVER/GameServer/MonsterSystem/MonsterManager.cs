using MMORPG.Common.Proto.Entity;
using GameServer.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GameServer.MapSystem;
using GameServer.EntitySystem;
using GameServer.Manager;

namespace GameServer.MonsterSystem
{
    /// <summary>
    /// 怪物管理器
    /// 负责管理地图内的所有怪物
    /// </summary>
    public class MonsterManager
    {
        private Dictionary<int, Monster> _monsterDict = new();
        private Map _map;

        public MonsterManager(Map map)
        {
            _map = map;
        }

        public void Start()
        {

        }

        public void Update()
        {
        }

        /// <summary>
        /// 创建并初始化一个新的怪物对象。
        /// </summary>
        /// <param name="spawnDefine">怪物的生成定义，包含生成位置等信息。</param>
        /// <param name="unitId">怪物的单位ID，用于从数据管理器中获取单位数据。</param>
        /// <param name="pos">怪物的位置。</param>
        /// <param name="dire">怪物的方向。</param>
        /// <param name="name">怪物的名称。</param>
        /// <param name="level">怪物的等级。</param>
        /// <returns>返回创建的怪物对象。</returns>
        public Monster NewMonster(SpawnDefine spawnDefine, int unitId, Vector3 pos, Vector3 dire, string name, int level)
        {
            // 创建一个新的怪物对象。
            var monster = new Monster(spawnDefine, EntityManager.Instance.NewEntityId(), DataManager.Instance.UnitDict[unitId], _map, pos, dire, name, level);
            
            // 将新创建的怪物对象添加到实体管理器中。
            EntityManager.Instance.AddEntity(monster);
            
            // 将新创建的怪物对象添加到怪物字典中，以便后续快速查找。
            _monsterDict.Add(monster.EntityId, monster);
            
            // 通知地图有新的实体进入，以便地图可以对其进行管理。
            _map.EntityEnter(monster);
            
            // 启动怪物对象，开始其生命周期。
            monster.Start();
            
            // 返回创建的怪物对象。
            return monster;
        }

    }
}
