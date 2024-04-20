using Common.Proto.Player;
using QFramework;
using Tool;
using UnityEngine;


namespace MMORPG
{
    /// <summary>
    /// ��ͼ������
    /// ��������ڵ�ǰ��ͼ�д�����ɫ���¼�������ɫ���뵽��ͼ
    /// </summary>
    public class SpaceManager : MonoBehaviour, IController, ICanSendEvent
    {
        public IArchitecture GetArchitecture()
        {
            return GameApp.Interface;
        }

        async void Start()
        {
            var box = this.GetSystem<IBoxSystem>();
            var net = this.GetSystem<INetworkSystem>();
            box.ShowSpinner("");
            net.SendToServer(new EnterGameRequest
            {
                CharacterId = 1,
            });
            var response = await net.ReceiveAsync<EnterGameResponse>();
            box.CloseSpinner();
            this.GetSystem<IPlayerManagerSystem>().SetMineId(response.Character.Entity.EntityId);
        }
    }
}