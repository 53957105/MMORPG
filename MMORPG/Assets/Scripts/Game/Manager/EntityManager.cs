using Common.Proto.Event;
using Common.Proto.Event.Map;
using MMORPG;
using MoonSharp.VsCodeDebugger.SDK;
using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tool;
using UnityEngine;

public struct NetworkControlData
{
    public float DeltaTime;
}

public struct NetworkSyncData
{
    public Vector3 Postion;
    public Quaternion Rotation;
}


public interface INetworkEntityCallbacks
{
    public void NetworkMineUpdate() { }
    public void NetworkMineFixedUpdate() { }
    public void NetworkSyncUpdate(NetworkSyncData data) { }
}


public class EntityManager : MonoBehaviour, IController, ICanSendEvent
{
    private IEntityManagerSystem _entityManager;
    private ResLoader _resLoader = ResLoader.Allocate();

    private void Awake()
    {
        _entityManager = this.GetSystem<IEntityManagerSystem>();

        this.GetSystem<INetworkSystem>().ReceiveEvent<EntityEnterResponse>(OnEntityEnterReceived)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
        this.GetSystem<INetworkSystem>().ReceiveEvent<EntityTransformSyncResponse>(OnEntitySyncReceived)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private void OnEntityEnterReceived(EntityEnterResponse response)
    {
        foreach (var netEntity in response.Datas)
        {
            var entityId = netEntity.EntityId;
            var position = netEntity.Transform.Position.ToVector3();
            var rotation = Quaternion.Euler(netEntity.Transform.Direction.ToVector3());

            //TODO 根据Entity加载对应的Prefab
            _entityManager.SpawnEntity(_resLoader.LoadSync<Entity>("HeroKnightFemale"), entityId, position, rotation, false);
        }
    }

    private void OnEntitySyncReceived(EntityTransformSyncResponse response)
    {
        var entityId = response.EntityId;
        var position = response.Transform.Position.ToVector3();
        var rotation = Quaternion.Euler(response.Transform.Direction.ToVector3());
        var entity = _entityManager.GetEntityDict(false)[entityId];

        var data = new NetworkSyncData
        {
            Postion = position,
            Rotation = rotation
        };

        entity.GetComponents<INetworkEntityCallbacks>().ForEach(cb => {
            cb.NetworkSyncUpdate(data);
        });
    }

    public IArchitecture GetArchitecture()
    {
        return GameApp.Interface;
    }
}