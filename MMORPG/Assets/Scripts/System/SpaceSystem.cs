using MoonSharp.VsCodeDebugger.SDK;
using QFramework;
using System.Collections;
using System.Collections.Generic;
using ThirdPersonCamera;
using UnityEngine;



/// <summary>
/// ��ͼϵͳ
/// �������Entity���뼰�˳���ͼ
/// </summary>
public class SpaceSystem : AbstractSystem
{
    private int _spaceId;
    public int SpaceId { get { return _spaceId; } }

    protected override void OnInit()
    {
        _spaceId = 1;
    }

    public void CharacterEnter(NetPlayer player)
    {
        this.SendEvent(new CharacterEnterEvent() { Player = player });
        FSM
    }

    public void CharacterLeave(NetPlayer player)
    {
        // this.SendEvent(new CharacterLeaveEvent() { Player = player });
    }
}

