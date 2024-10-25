using System.Diagnostics;
using Aoi;
using MMORPG.Common.Proto.Entity;
using MMORPG.Common.Proto.Map;
using GameServer.Tool;
using Serilog;
using Google.Protobuf;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Principal;
using GameServer.NpcSystem;
using GameServer.EntitySystem;
using GameServer.FightSystem;
using GameServer.InventorySystem;
using GameServer.PlayerSystem;
using GameServer.MissileSystem;
using GameServer.MonsterSystem;

namespace GameServer.MapSystem
{
    public class Map
    {
        const int InvalidMapId = 0;

        public MapDefine Define { get; }
        public int MapId => Define.ID;

        public PlayerManager PlayerManager { get; }
        public MonsterManager MonsterManager { get; }
        public NpcManager NpcManager { get; }
        public MissileManager MissileManager { get; }
        public SpawnManager SpawnManager { get; }
        public DroppedItemManager DroppedItemManager { get; }

        private AoiWord _aoiWord;

        public override string ToString()
        {
            return $"Map:\"{Define.Name}:{MapId}\"";
        }

        // 定义地图类的构造函数
        // 该构造函数用于初始化地图及其内部的各种管理器
        // 参数 mapDefine: 包含地图基本信息的定义对象，用于配置地图属性
        public Map(MapDefine mapDefine)
        {
            // 将传入的地图定义对象赋值给地图实例的Define属性
            Define = mapDefine;
        
            // 初始化AOI（Area of Interest）世界对象，用于管理游戏中的可见区域
            // 参数20表示AOI世界的初始容量
            _aoiWord = new(20);
        
            // 初始化玩家管理器，负责处理地图中所有玩家的相关操作
            PlayerManager = new(this);
            // 初始化怪物管理器，负责处理地图中所有怪物的行为和状态
            MonsterManager = new(this);
            // 初始化NPC管理器，用于管理地图中所有非玩家角色
            NpcManager = new(this);
            // 初始化导弹管理器，负责跟踪和更新地图中所有导弹的动态
            MissileManager = new(this);
            // 初始化生成管理器，用于控制地图中怪物、NPC等的生成逻辑
            SpawnManager = new(this);
            // 初始化掉落物品管理器，负责管理地图中所有掉落物品的状态
            DroppedItemManager = new(this);
        }

        /// <summary>
        /// 启动游戏中的各个管理器
        /// </summary>
        public void Start()
        {
            // 启动玩家管理器，处理玩家相关的初始化和启动逻辑
            PlayerManager.Start();
            
            // 启动怪物管理器，处理怪物相关的初始化和启动逻辑
            MonsterManager.Start();
            
            // 启动NPC管理器，处理NPC相关的初始化和启动逻辑
            NpcManager.Start();
            
            // 启动导弹管理器，处理导弹相关的初始化和启动逻辑
            MissileManager.Start();
            
            // 启动生成管理器，处理游戏世界中各种对象的生成逻辑
            SpawnManager.Start();
            
            // 启动掉落物品管理器，处理掉落物品相关的初始化和启动逻辑
            DroppedItemManager.Start();
        }

        public void Update()
        {
            PlayerManager.Update();
            MonsterManager.Update();
            NpcManager.Update();
            MissileManager.Update();
            DroppedItemManager.Update();
            SpawnManager.Update();
        }

        /// <summary>
        /// 广播实体进入场景
        /// </summary>
        public void EntityEnter(Entity entity)
        {
            Log.Information($"{entity}进入{entity.Map}");

            entity.AoiEntity = _aoiWord.Enter(entity.EntityId, entity.Position.X, entity.Position.Y);
            
            var res = new EntityEnterResponse();
            res.Datas.Add(ConstructEntityEnterData(entity));

            // 向能观察到新实体的角色广播新实体加入场景
            PlayerManager.Broadcast(res, entity);

            // 如果新实体是玩家
            // 向新玩家投递已在场景中的在其可视范围内的实体
            if (entity.EntityType == EntityType.Player)
            {
                res.Datas.Clear();
                ScanEntityFollowing(entity, e =>
                {
                    res.Datas.Add(ConstructEntityEnterData(e));
                });
                var currentPlayer = entity as Player;
                currentPlayer?.User.Channel.Send(res);
            }
        }

        private EntityEnterData ConstructEntityEnterData(Entity entity)
        {
            var data = new EntityEnterData()
            {
                EntityId = entity.EntityId,
                UnitId = entity.UnitDefine.ID,
                EntityType = entity.EntityType,
                Transform = ProtoHelper.ToNetTransform(entity.Position, entity.Direction),
            };
            if (entity is Actor actor)
            {
                data.Actor = actor.ToNetActor();
            }
            return data;
        }

        /// <summary>
        ///  广播实体离开场景
        /// </summary>
        public void EntityLeave(Entity entity)
        {
            Log.Information($"{entity}离开{entity.Map}");

            // 向能观察到实体的角色广播实体离开场景
            // 实际上直接广播是向当前entity的关注实体广播而非关注当前entity的实体
            // 如果所有实体的视野范围一致则没有这个问题，但如果不一致的话，需要考虑另行维护
            var res = new EntityLeaveResponse();
            res.EntityIds.Add(entity.EntityId);
            PlayerManager.Broadcast(res, entity);
            _aoiWord.Leave(entity.AoiEntity);
        }

        /// <summary>
        /// 同步实体位置并向能观察到该实体的玩家广播消息
        /// </summary>
        /// <param name="entity"></param>
        public void EntityRefreshPosition(Entity entity)
        {
            var enterRes = new EntityEnterResponse();
            enterRes.Datas.Add(ConstructEntityEnterData(entity));

            var leaveRes = new EntityLeaveResponse();
            leaveRes.EntityIds.Add(entity.EntityId);

            bool init1 = false, init2 = false;

            _aoiWord.Refresh(entity.AoiEntity, entity.Position.X, entity.Position.Y,
                entityId =>
                {
                    if (init1 == false)
                    {
                        enterRes.Datas.Clear();
                        init1 = true;
                    }
                    // 如果移动的是玩家，还需要向该玩家通知所有新加入视野范围的实体
                    // Log.Debug($"[Map.EntityRefreshPosition]2.实体：{entityId} 进入了 实体：{entity.EntityId} 的视距范围");
                    if (entity.EntityType != EntityType.Player) return;
                    var enterEntity = EntityManager.Instance.GetEntity(entityId);
                    Debug.Assert(enterEntity != null);
                    enterRes.Datas.Add(ConstructEntityEnterData(enterEntity));
                },
                entityId =>
                {
                    if (init2 == false)
                    {
                        leaveRes.EntityIds.Clear();
                        init2 = true;
                    }
                    // 如果移动的是玩家，还需要向该玩家通知所有退出其视野范围的实体
                    // Log.Debug($"[Map.EntityRefreshPosition]2.实体：{entityId} 离开了 实体：{entity.EntityId} 的视距范围");
                    if (entity.EntityType != EntityType.Player) return;
                    var leaveEntity = EntityManager.Instance.GetEntity(entityId);
                    Debug.Assert(leaveEntity != null);
                    leaveRes.EntityIds.Add(leaveEntity.EntityId);
                },
                entityId =>
                {
                    // 如果进入了一个玩家的视距范围，则向其通知有实体加入
                    // Log.Debug($"[Map.EntityRefreshPosition]1.实体：{entity.EntityId} 进入了 实体：{entityId} 的视距范围");
                    var enterEntity = EntityManager.Instance.GetEntity(entityId);
                    Debug.Assert(enterEntity != null);
                    if (enterEntity.EntityType != EntityType.Player) return;

                    var player = enterEntity as Player;
                    Debug.Assert(player != null);
                    player?.User.Channel.Send(enterRes);
                },
                entityId =>
                {
                    // 如果离开了一个玩家的视距范围，则向其通知有实体退出
                    // Log.Debug($"[Map.EntityRefreshPosition]1.实体：{entity.EntityId} 离开了 实体：{entityId} 的视距范围");
                    var leaveEntity = EntityManager.Instance.GetEntity(entityId);
                    Debug.Assert(leaveEntity != null);
                    if (leaveEntity.EntityType != EntityType.Player) return;

                    var player = leaveEntity as Player;
                    Debug.Assert(player != null);
                    player?.User.Channel.Send(leaveRes);
                });
        
            if (entity.EntityType == EntityType.Player)
            {
                var player = entity as Player;
                Debug.Assert(player != null);
                if (init1)
                {
                    player?.User.Channel.Send(enterRes);
                }
                if (init2)
                {
                    player?.User.Channel.Send(leaveRes);
                }
            }
        }

        /// <summary>
        /// 获取指定实体视距范围内实体
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public void ScanEntityFollowing(Entity entity, Action<Entity> callback)
        {
            _aoiWord.ScanFollowingList(entity.AoiEntity, followingEntityId =>
            {
                var followingEntity = EntityManager.Instance.GetEntity(followingEntityId);
                if (followingEntity != null) callback(followingEntity);
            });
        }

        /// <summary>
        /// 按半径获取指定实体视距范围内实体
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public void ScanEntityFollowing(Entity entity, float range, Action<Entity> callback)
        {
        }


        /// <summary>
        /// 获取指定实体视距范围内实体
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public void ScanEntityFollower(Entity entity, Action<Entity> callback)
        {
            _aoiWord.ScanFollowerList(entity.AoiEntity, followerEntityId =>
            {
                var followerEntity = EntityManager.Instance.GetEntity(followerEntityId);
                if (followerEntity != null) callback(followerEntity);
            });
        }

        public Entity? GetEntityFollowingNearest(Entity entity, Predicate<Entity>? condition = null)
        {
            Entity? nearest = null;
            float minDistance = 0;
            _aoiWord.ScanFollowerList(entity.AoiEntity, followerEntityId =>
            {
                var followerEntity = EntityManager.Instance.GetEntity(followerEntityId);
                if (followerEntity != null && (condition == null || condition(followerEntity)))
                {
                    if (nearest == null)
                    {
                        nearest = followerEntity;
                        minDistance = Vector2.Distance(followerEntity.Position, entity.Position);
                    }
                    else
                    {
                        var tmp = Vector2.Distance(followerEntity.Position, entity.Position);
                        if (tmp < minDistance)
                        {
                            nearest = followerEntity;
                            minDistance = tmp;
                        }
                    }
                }
            });
            return nearest;
        }

        /// <summary>
        /// 根据网络实体对象更新实体并广播新状态
        /// </summary>
        public void EntityTransformSync(int entityId, NetTransform transform, int stateId, ByteString data)
        {
            var entity = EntityManager.Instance.GetEntity(entityId);
            if (entity == null) return;

            entity.Position = transform.Position.ToVector3().ToVector2();
            entity.Direction = transform.Direction.ToVector3();
            EntityRefreshPosition(entity);

            var response = new EntityTransformSyncResponse
            {
                EntityId = entityId,
                Transform = transform,
                StateId = stateId,
                Data = data
            };

            // 向所有角色广播新实体的状态更新
            PlayerManager.Broadcast(response, entity);
        }

        /// <summary>
        /// 根据服务器实体对象更新实体并广播新状态
        /// </summary>
        public void EntitySync(int entityId, int stateId)
        {
            var entity = EntityManager.Instance.GetEntity(entityId);
            if (entity == null) return;
            EntityRefreshPosition(entity);

            var response = new EntityTransformSyncResponse
            {
                EntityId = entityId,
                Transform = new()
                {
                    Direction = entity.Position.ToVector3().ToNetVector3(),
                    Position = entity.Direction.ToNetVector3()
                },
                StateId = stateId,
                Data = null
            };

            // 向所有角色广播新实体的状态更新
            PlayerManager.Broadcast(response, entity);
        }

    }
}
