using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]     // 속성 선언
public class MoveCharacter : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;        // 이동속도 선언
    private Vector3 moveForce;      // 이동 힘
    [SerializeField]
    private float jump;
    [SerializeField]
    private float gravity;

    private CharacterController characterController;    // 이동 제어를 위한 컴포넌트

    public float MoveSpeed
    {
        set => moveSpeed = Mathf.Max(0, value); // 속도는 음수X
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
        characterController.Move(moveForce * Time.deltaTime);   // 초당 이동거리(속도)
    }
    public void MoveTo(Vector3 direction)
    {
        // 이동 방향
        direction = transform.rotation * new Vector3(direction.x, 0, direction.z);

        // 이동 벡터(힘)
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
