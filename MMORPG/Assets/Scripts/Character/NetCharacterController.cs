using QFramework;
using ThirdPersonCamera;
using UnityEngine;


/// <summary>
/// ��ɫ�����������������ҶԽ�ɫ�����룬����ɫ�ı���
/// </summary>
public class NetCharacterController : MonoBehaviour, IController
{
    // NetPlayerӦ������Model�㣬������ֱ����
    public NetPlayer NetPlayer;

    private NetCharacterAnimator _animator;

    private float _rotateLerp = 0.2f;
    private float _moveLerp = 0.2f;

    public IArchitecture GetArchitecture()
    {
        return GameApp.Interface;
    }

    private void Awake()
    {
        _animator = GetComponent<NetCharacterAnimator>();
    }

    private void Start()
    {
        if (NetPlayer.IsMine)
        {
            var camera = Camera.main.GetComponent<CameraController>();
            camera.InitFromTarget(transform);
        }
    }

    private void FixedUpdate()
    {
        if (NetPlayer.IsMine)
        {
            ControlCharacter();
        }
    }

    private void Update()
    {
        if (!NetPlayer.IsMine)
        {
            SyncCharacter();
        }
    }

    protected virtual void ControlCharacter()
    {
        if (Input.GetMouseButton(0))
        {
            _animator.PlayAttack01();
        }

        else if (_animator.Status != NetCharacterAnimator.AnimationStatus.Attack)
        {
            // ��ȡ�������ǰ���򣬼�������Ĺ۲췽��
            var cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0f; // ����y�ᣬ������ˮƽ����

            // ��ȡ���뷽��
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");
            Vector3 inputDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

            Vector3 moveDirection = Vector3.zero;
            if (inputDirection != Vector3.zero)
            {
                moveDirection = Quaternion.LookRotation(cameraForward) * inputDirection;

                _animator.PlayRun();
            }
            else
            {
                _animator.PlayIdle();
            }

            CharacterController controller = GetComponent<CharacterController>();
            controller.SimpleMove(moveDirection * NetPlayer.MoveSpeed);

            // ��������룬��ת��ɫ�����ƶ�����
            if (inputDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized, Vector3.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, _rotateLerp);
            }

            this.SendCommand(new CharacterPositionChangeCommand(NetPlayer, transform.position, transform.rotation));
        }
    }

    protected virtual void SyncCharacter()
    {
        // ��������ң�����λ��ͬ��
        //transform.position = NetPlayer.Position;
        transform.position = Vector3.Lerp(transform.position, NetPlayer.Position, _moveLerp);
        transform.rotation = Quaternion.Lerp(transform.rotation, NetPlayer.Rotation, _rotateLerp);
    }
}
