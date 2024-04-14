using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetCharacterController : MonoBehaviour
{
    public float MoveSpeed;
    public float RotateLerp;

    protected bool _isMine = true;

    private NetCharacterAnimator _animator;

    private void Awake()
    {
        _animator = GetComponent<NetCharacterAnimator>();
    }

    void FixedUpdate()
    {
        if (_isMine)
        {
            ControlPlayer();


        }
    }

    protected virtual void ControlPlayer()
    {
        if (Input.GetMouseButton(0))
        {
            PlayerAttack();
        }

        else if(_animator.Status != NetCharacterAnimator.AnimationStatus.Attack) {
            // ��ȡ�������ǰ���򣬼�������Ĺ۲췽��
            var cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0f; // ����y�ᣬ������ˮƽ����

            CharacterController controller = GetComponent<CharacterController>();

            // ��ȡ���뷽��
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");
            Vector3 inputDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

            Vector3 moveDirection = Vector3.zero;
            if (inputDirection != Vector3.zero)
            {
                moveDirection = Quaternion.LookRotation(cameraForward) * inputDirection;

                PlayerRun();
            }
            else
            {
                PlayerIdle();
            }

            controller.SimpleMove(moveDirection * MoveSpeed);

            // ��������룬��ת��ɫ�����ƶ�����
            if (inputDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized, Vector3.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, RotateLerp);
            }
        }
    }

    protected virtual void PlayerAttack()
    {
        _animator.PlayAttack01();
    }

    protected virtual void PlayerRun()
    {
        _animator.PlayRun();
    }

    protected virtual void PlayerIdle()
    {
        _animator.PlayIdle();
    }

}
