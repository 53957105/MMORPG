using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �Զ�ͬ��
/// </summary>
[RequireComponent(typeof(Entity))]
public class AutoSync : NetworkBehaviour, INetworkEntityCallbacks
{
    public void NetworkControlFixedUpdate(Entity entity, float deltaTime)
    {
        entity.NetworkUpdatePositionAndRotation();
    }

    public void NetworkSyncUpdate(Entity entity, EntitySyncData data)
    {
        entity.transform.SetPositionAndRotation(data.Postion, data.Rotation);
    }
}
