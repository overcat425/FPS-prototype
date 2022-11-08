using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]     // �Ӽ� ����
public class MoveCharacter : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;        // �̵��ӵ� ����
    private Vector3 moveForce;      // �̵� ��
    [SerializeField]
    private float jump;
    [SerializeField]
    private float gravity;

    private CharacterController characterController;    // �̵� ��� ���� ������Ʈ

    public float MoveSpeed
    {
        set => moveSpeed = Mathf.Max(0, value); // �ӵ��� ����X
        get => moveSpeed;
    }
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }
    private void Update()
    {
        if (!characterController.isGrounded)
        {
            moveForce.y += gravity * Time.deltaTime;
        }
        characterController.Move(moveForce * Time.deltaTime);   // �ʴ� �̵��Ÿ�(�ӵ�)
    }
    public void MoveTo(Vector3 direction)
    {
        // �̵� ����
        direction = transform.rotation * new Vector3(direction.x, 0, direction.z);

        // �̵� ����(��)
        moveForce = new Vector3(direction.x * moveSpeed, moveForce.y, direction.z * moveSpeed);
    }
    public void Jump()
    {
        if (characterController.isGrounded)
        {
            moveForce.y = jump;
        }
    }
}
