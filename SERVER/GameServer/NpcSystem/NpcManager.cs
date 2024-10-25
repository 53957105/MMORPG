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

namespace GameServer.NpcSystem
{
    /// <summary>
    /// Npc管理器
    /// 负责管理地图内的所有Npc
    /// </summary>
    public class NpcManager
    {
        private Dictionary<int, Npc> _npcDict = new();
        private Map _map;

        public NpcManager(Map map)
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
        /// 创建并初始化一个新的NPC对象。
        /// </summary>
        /// <param name="unitId">NPC的单位ID，用于从数据管理器中获取NPC的属性信息。</param>
        /// <param name="pos">NPC的位置。</param>
        /// <param name="dire">NPC的方向。</param>
        /// <param name="name">NPC的名称。</param>
        /// <param name="level">NPC的等级。</param>
        /// <returns>返回创建的NPC对象。</returns>
        public Npc NewNpc(int unitId, Vector3 pos, Vector3 dire, string name, int level)
        {
            // 创建一个新的NPC实例，传入必要的参数。
            var npc = new Npc(EntityManager.Instance.NewEntityId(), DataManager.Instance.UnitDict[unitId], _map, name, pos, dire, level);
            
            // 将新创建的NPC添加到实体管理器中，以便进行统一管理。
            EntityManager.Instance.AddEntity(npc);
        
            // 将NPC添加到NPC字典中，以便快速查找和访问。
            _npcDict.Add(npc.EntityId, npc);
            
            // 通知地图有新的实体进入，以便地图可以对其进行管理和渲染。
            _map.EntityEnter(npc);
        
            // 启动NPC，执行NPC初始化逻辑。
            npc.Start();
            
            // 返回创建的NPC实例。
            return npc;
        }

    }
}
