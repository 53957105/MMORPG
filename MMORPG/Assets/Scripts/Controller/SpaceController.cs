using QFramework;
using UnityEngine;


/// <summary>
/// ��ͼ������
/// ��������ڵ�ǰ��ͼ�д�����ɫ���¼�������ɫ���뵽��ͼ
/// </summary>
public class SpaceController : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture()
    {
        return GameApp.Interface;
    }

    void Start()
    {
        this.RegisterEvent<CharacterEnterEvent>(e =>
        {
            SceneHelperController.Instance.Invoke(() =>
            {
                var playerGo = Instantiate(Resources.Load<GameObject>("Prefabs/Character/Player/DogPBR"),
                e.Player.Position, e.Player.Rotation);
                playerGo.GetComponent<NetCharacterController>().NetPlayer = e.Player;
            });
        }).UnRegisterWhenGameObjectDestroyed(gameObject);
    }
}
