using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �Զ�ͬ��
/// </summary>
[RequireComponent(typeof(Entity))]
public class AutoSync : MonoBehaviour, INetworkEntityCallbacks
{
    void INetworkEntityCallbacks.NetworkControlFixedUpdate(NetworkControlData data)
    {
    }

    void INetworkEntityCallbacks.NetworkSyncUpdate(NetworkSyncData data)
    {
        transform.SetPositionAndRotation(data.Postion, data.Rotation);
    }
}
