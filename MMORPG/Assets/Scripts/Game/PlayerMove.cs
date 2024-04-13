using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMove : MonoBehaviour
{
    public GameObject Player;
    public CinemachineFreeLook PlayerCamera;
    public float MoveSpeed;

    void FixedUpdate()
    {
        // ��ȡ�������ǰ���򣬼�������Ĺ۲췽��
        Vector3 cameraForward = PlayerCamera.transform.forward;
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

        var rb = Player.GetComponent<Rigidbody>();
        // �ƶ���ɫ
        rb.velocity = moveDirection * MoveSpeed;

        // ��������룬��ת��ɫ�����ƶ�����
        if (inputDirection != Vector3.zero)
        {
            Player.transform.forward = moveDirection.normalized;
        }
    }
}
