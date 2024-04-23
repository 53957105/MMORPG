using Common.Proto.Player;
using Common.Tool;
using QFramework;
using System.Threading.Tasks;
using ThirdPersonCamera;
using Tool;
using UnityEngine;


namespace MMORPG
{
    /// <summary>
    /// ��ͼ������
    /// ��������ڵ�ǰ��ͼ�д�����ɫ���¼�������ɫ���뵽��ͼ
    /// </summary>
    public class MapManager : MonoBehaviour, IController
    {
        private IPlayerManagerSystem _playerManager;
        private IEntityManagerSystem _entityManager;
        private ResLoader _resLoader = ResLoader.Allocate();

        public IArchitecture GetArchitecture()
        {
            return GameApp.Interface;
        }

        void Awake()
        {
            _playerManager = this.GetSystem<IPlayerManagerSystem>();
            _entityManager = this.GetSystem<IEntityManagerSystem>();
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
            if (response.Error != Common.Proto.Base.NetError.Success)
            {
                Logger.Error("Network", $"EnterGame Error:{response.Error.GetInfo().Description}");
                //TODO Error����
                return;
            }

            Logger.Info("Network", $"EnterGame Success, MineId:{response.Character.Entity.EntityId}");
            _playerManager.SetMineId(response.Character.Entity.EntityId);

            var entity = _entityManager.SpawnEntity(
                _resLoader.LoadSync<Entity>("DogPBR"),
                response.Character.Entity.EntityId,
                response.Character.Entity.Position.ToVector3(),
                Quaternion.Euler(response.Character.Entity.Direction.ToVector3()),
                true);

            Camera.main.GetComponent<CameraController>().InitFromTarget(entity.transform);
        }
    }
}