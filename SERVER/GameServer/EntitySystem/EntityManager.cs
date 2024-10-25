using GameServer.Tool;
using System.Security.Principal;

namespace GameServer.EntitySystem
{
    /// <summary>
    /// 实体管理器
    /// 负责管理整个游戏的所有实体
    /// </summary>
    public class EntityManager : Singleton<EntityManager>
    {
        // 定义一个私有整型变量_serialNum，用于实体的序列号分配
        private int _serialNum = 0;
        
        // 创建一个字典_entityDict，用于存储所有实体，键为实体的ID，值为实体对象
        private Dictionary<int, Entity> _entityDict = new();
        
        // 创建一个字典_addQueue，用于临时存储待添加的实体，键为实体的ID，值为实体对象
        private Dictionary<int, Entity> _addQueue = new();
        
        // 创建一个字典_removeQueue，用于临时存储待移除的实体，键为实体的ID，值为实体对象
        private Dictionary<int, Entity> _removeQueue = new();

        private EntityManager() { }

        public void Start() { }

        /// <summary>
        /// 执行实体的更新逻辑。
        /// </summary>
        public void Update()
        {
            // 遍历所有实体并执行它们的更新逻辑
            foreach (var entity in _entityDict.Values)
            {
                entity.Update();
            }
        
            // 将待添加的实体加入到实体字典中
            foreach (var entity in _addQueue.Values)
            {
                _entityDict[entity.EntityId] = entity;
            }
            // 清空添加队列，准备下一次添加操作
            _addQueue.Clear();
        
            // 从实体字典中移除待删除的实体
            foreach (var entity in _removeQueue.Values)
            {
                _entityDict.Remove(entity.EntityId);
            }
            // 清空删除队列，准备下一次删除操作
            _removeQueue.Clear();
        }

        public int NewEntityId()
        {
            return ++_serialNum;
        }

        public void AddEntity(Entity entity)
        {
            _addQueue[entity.EntityId] = entity;
        }

        public void RemoveEntity(Entity entity)
        {
            _removeQueue[entity.EntityId] = entity;
        }

        public Entity? GetEntity(int entityId)
        {
            _entityDict.TryGetValue(entityId, out var entity);
            if (entity != null)
            {
                if (_removeQueue.ContainsKey(entityId)) return null;
            }
            else
            {
                _addQueue.TryGetValue(entityId, out entity);
            }
            return entity;
        }

        public bool IsValidEntity(int entityId)
        {
            var vaild =  _entityDict.ContainsKey(entityId);
            if (vaild) return !_removeQueue.ContainsKey(entityId);
            else return _addQueue.ContainsKey(entityId);
            
        }
    }
}
