using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetPlayer : NetObject
{
    public float MoveSpeed = 5;
    public float RotateLerp = 0.5f;

    private Rigidbody _rigidbody;

    protected bool _isMine = true;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
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
        // ��ȡ�������ǰ���򣬼�������Ĺ۲췽��
        var cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f; // ����y�ᣬ������ˮƽ����

        // ��ȡ���뷽��
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // ��������룬�����ƶ����򣬻����������ǰ����
        Vector3 moveDirection = Vector3.zero;
        if (inputDirection != Vector3.zero)
        {
            moveDirection = Quaternion.LookRotation(cameraForward) * inputDirection;
        }
        // �ƶ���ɫ
        _rigidbody.velocity = moveDirection * MoveSpeed;

        // ��������룬��ת��ɫ�����ƶ�����
        if (inputDirection != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, moveDirection.normalized, RotateLerp);
        }
    }
}
