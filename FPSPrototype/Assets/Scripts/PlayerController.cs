using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Input KeyCodes")]
    [SerializeField]
    private KeyCode keyCodeRun = KeyCode.LeftShift; // 쉬프트로 달리기
    [SerializeField]
    private KeyCode keyCodeJump = KeyCode.Space;
    [SerializeField]
    private KeyCode keyCodeReload = KeyCode.R;

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipWalk;
    [SerializeField]
    private AudioClip audioClipRun;

    [SerializeField]
    private GameObject go_BaseUi;

    private RotateToMouse rotateToMouse;    // 마우스를 이용한 카메라 회전
    private MoveCharacter movement;         // 플레이어 이동, 점프
    private PlayerStatus status;
    private AudioSource audioSource;
    private WeaponBase weapon;

    public static bool canPlayerMove = false;
    public static bool isPause = false;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        isPause = false;
        rotateToMouse = GetComponent<RotateToMouse>();
        movement = GetComponent<MoveCharacter>();
        status = GetComponent<PlayerStatus>();
        audioSource = GetComponent<AudioSource>();
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (canPlayerMove)
        {
            UpdateRotate();
            UpdateMove();
            UpdateJump();
            UpdateWeaponAction();
        }
        if (isPause)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            canPlayerMove = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            canPlayerMove = true;
        }
    }
    
    private void UpdateRotate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        rotateToMouse.UpdateRotate(mouseX, mouseY);
    }
    private void UpdateMove()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        if(x!=0 || z != 0)
        {
            bool isRun = false;
            if (z > 0) isRun = Input.GetKey(keyCodeRun);
            movement.MoveSpeed = isRun == true ? status.RunSpeed : status.WalkSpeed;
            weapon.Animator.MoveSpeed = isRun == true ? 1 : 0.5f;
            audioSource.clip = isRun == true ? audioClipRun : audioClipWalk;

            if(audioSource.isPlaying == false)
            {
                audioSource.loop = true;
                audioSource.Play();
            }
        }else
        {
            movement.MoveSpeed = 0;
            weapon.Animator.MoveSpeed = 0;
            if(audioSource == true)
            {
                audioSource.Stop();
            }
        }

        movement.MoveTo(new Vector3(x, 0, z));
    }
    private void UpdateJump()
    {
        if (Input.GetKeyDown(keyCodeJump))
        {
            movement.Jump();
        }
    }
    private void UpdateWeaponAction()
    {
        if (Input.GetMouseButtonDown(0))
        {
            weapon.StartWeapon();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            weapon.StopWeapon();
        }
        if (Input.GetMouseButtonDown(1))
        {
            weapon.StartWeapon(1);
        }else if (Input.GetMouseButtonUp(1))
        {
            weapon.StopWeapon(1);
        }
        if (Input.GetKeyDown(keyCodeReload))
        {
            weapon.StartReload();
        }
    }
    public void TakeDamage(int damage)
    {
        bool isDie = status.DecreaseHP(damage);
        if(isDie == true)
        {
            Debug.Log("Game Over");
            SceneManager.LoadScene("GameOver");
        }
    }
    public void SwitchingWeapon(WeaponBase newWeapon)
    {
        weapon = newWeapon;
    }
}
