using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ��ɫϵͳ
/// �������������ҵĽ�ɫ
/// ��¼��ɫ��Ϣ������λ�á����Ե�
/// CharacterSystemͨ������CharacterPositionChangeEvent�¼���ʾ��ɫλ�ø������
/// </summary>
public class CharacterSystem : AbstractSystem
{
    private Dictionary<int, NetPlayer> _playerSet;

    protected override void OnInit()
    {
        _playerSet = new();

        this.RegisterEvent<CharacterEnterEvent>(e =>
        {
           AddPlayer(e.Player);
        });
    }

    public void PositionChange(NetPlayer player, Vector3 position, Quaternion rotation)
    {
        player.Position = position;
        player.Rotation = rotation;
        // this.SendEvent(new CharacterPositionChangeEvent() { Player = player });
    }

    public NetPlayer GetPlayer(int entityId)
    {
        lock (_playerSet)
        {
            return _playerSet[entityId];
        }
    }

    public void AddPlayer(NetPlayer player)
    {
        lock (_playerSet)
        {
            _playerSet[player.EntityId] = player;
        }
    }
}
