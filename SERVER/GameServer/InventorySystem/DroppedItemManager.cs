using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GameServer.Db;
using GameServer.EntitySystem;
using GameServer.Manager;
using GameServer.MapSystem;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;

namespace GameServer.InventorySystem
{
    public class DroppedItemManager
    {
        private Map _map;
        private Dictionary<int, DroppedItem> _itemDict = new();
        Random _random = new();

        public DroppedItemManager(Map map)
        {
            _map = map;
        }

        public void Start()
        {
            NewDroppedItem(1001, new(0, 5, 0), Vector3.Zero, 1);
            NewDroppedItem(1007, new(5, 5, 0), Vector3.Zero, 1);
        }

        public void Update()
        {
        }

        /// <summary>
        /// 创建一个新的掉落物品实例。
        /// </summary>
        /// <param name="itemId">物品的ID。</param>
        /// <param name="pos">物品在地图上的位置。</param>
        /// <param name="dire">物品的移动方向。</param>
        /// <param name="amount">物品的数量。</param>
        /// <returns>返回新创建的掉落物品实例。</returns>
        public DroppedItem NewDroppedItem(int itemId, Vector3 pos, Vector3 dire, int amount)
        {
            // 创建一个新的掉落物品实体，使用EntityManager来生成一个新的实体ID。
            // 通过itemId查找物品对应的单位信息，用于创建掉落物品。
            var item = new DroppedItem(EntityManager.Instance.NewEntityId(),
                DataManager.Instance.UnitDict[DataManager.Instance.ItemDict[itemId].UnitId], _map, pos, dire, itemId, amount);
            
            // 将新创建的掉落物品添加到EntityManager中，以便于游戏引擎统一管理。
            EntityManager.Instance.AddEntity(item);
        
            // 将新的掉落物品添加到本地字典中，以便快速查找和管理。
            _itemDict.Add(item.EntityId, item);
            
            // 通知地图有新的实体进入，以便地图可以对其进行管理和渲染。
            _map.EntityEnter(item);
        
            // 初始化掉落物品，开始其生命周期。
            item.Start();
            
            // 返回新创建的掉落物品实例。
            return item;
        }

        public DroppedItem NewDroppedItemWithOffset(int itemId, Vector3 pos, Vector3 dire, int amount, float offset)
        {
            
            // 位置偏移掉落
            float offsetX = _random.NextSingle() * offset * 2 - offset;
            float offsetZ = _random.NextSingle() * offset * 2 - offset;

            pos.X += offsetX;
            pos.Z += offsetZ;

            return NewDroppedItem(itemId, pos, Vector3.Zero, amount);
        }

        /// <summary>
        /// 删除掉落物
        /// </summary>
        /// <param name="player"></param>
        public void RemoveDroppedItem(DroppedItem item)
        {
            EntityManager.Instance.RemoveEntity(item);
            _itemDict.Remove(item.EntityId);
        }
    }
}
