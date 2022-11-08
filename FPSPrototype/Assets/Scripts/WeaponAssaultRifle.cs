using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponAssaultRifle : WeaponBase
{
    [Header("Fire effects")]
    [SerializeField]
    private GameObject muzzleFlashEffect;   // 총구 화염 이펙트

    [Header("Spawn Points")]
    [SerializeField]
    private Transform casingSpawnPoint;     // 탄피 생성위치
    [SerializeField]
    private Transform bulletSpawnPoint;     // 총알 생성위치

    [Header("Aim UI")]
    [SerializeField]
    private Image imageAim;                     // 에임 모드에 따라서 에임이미지 변경

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipTakeOutWeapon;   // 무기 꺼내는소리
    [SerializeField]
    private AudioClip audioClipFire;                    // 공격 사운드
    [SerializeField]
    private AudioClip audioClipReload;              // 재장전 사운드

    private bool isModeChange = false;          // 에임모드?
    private float defaultModeFOV = 60;          // 기본모드 FOV
    private float aimModeFOV = 30;              // 에임모드 FOV

    private CasingMemoryPool casingMemoryPool;  // 탄피 생성 후 관리
    private HitScanMemoryPool hitScanMemoryPool;// 공격 이펙트 생성 후 관리
    private Camera mainCamera;                  // 총알 광선 발사

    public Transform camRecoil;                 // 반동에 따른 캠 움직임
    public Vector3 recoilKickback;              // 반동 회복을 위한 벡터값
    public float recoilAmount;                  // 반동 수준 설정
    private void Awake()
    {
        base.Setup();
        casingMemoryPool = GetComponent<CasingMemoryPool>();
        hitScanMemoryPool = GetComponent<HitScanMemoryPool>();
        mainCamera = Camera.main;

        weaponSettings.currentAmmoPack = 0;
        weaponSettings.currentAmmo = 0;
    }
    public void Update()
    {
        RecoilBack();
    }

    private void OnEnable()
    {
        PlaySound(audioClipTakeOutWeapon);
        muzzleFlashEffect.SetActive(false);
        onAmmoPackEvent.Invoke(weaponSettings.currentAmmoPack);
        onAmmoEvent.Invoke(weaponSettings.currentAmmo, weaponSettings.maxAmmo);
        ResetVariables();
    }
    public override void StartWeapon(int type = 0)
    {
        if (isReload == true) return;
        if (isModeChange == true) return;
        if(type == 0)
        {
            if(weaponSettings.isAutomaticAttack == true)
            {
                isAttack = true;
                StartCoroutine("OnAttackLoop");
            }else
            {
                OnAttack();
            }
        }else
        {
            if(isAttack == true) return;
            StartCoroutine("OnModeChange");
        }
    }
    public override void StopWeapon(int type = 0)
    {
        if(type == 0)
        {
            isAttack = false;
            StopCoroutine("OnAttackLoop");
        }
    }
    public override void StartReload()
    {
        if (isReload == true || weaponSettings.currentAmmoPack <= 0) return;
        StopWeapon();
        StartCoroutine("OnReload");
    }
    private IEnumerator OnAttackLoop()
    {
        while (true)
        {
            OnAttack();
            yield return null;
        }
    }
    public void OnAttack()
    {
        if(Time.time - lastAttackTime > weaponSettings.attackRate)
        {
            if(animator.MoveSpeed > 0.5f)
            {
                return;
            }
            lastAttackTime = Time.time;
            if(weaponSettings.currentAmmo <= 0)
            {
                return;
            }
            weaponSettings.currentAmmo --;
            onAmmoEvent.Invoke(weaponSettings.currentAmmo, weaponSettings.maxAmmo);

            string animation = animator.AimModeIs == true ? "AimFire" : "Fire";
            animator.Play(animation, -1, 0);
            //animator.Play("Fire", -1, 0);
            if (animator.AimModeIs == false) StartCoroutine("OnMuzzleFlashEffect");

            //StartCoroutine("OnMuzzleFlashEffect");
            PlaySound(audioClipFire);
            casingMemoryPool.SpawnCasing(casingSpawnPoint.position, transform.right);
            TwoStepRaycast();
            Recoil();
        }
    }
    private IEnumerator OnMuzzleFlashEffect()
    {
        muzzleFlashEffect.SetActive(true);

        yield return new WaitForSeconds(weaponSettings.attackRate * 0.3f);
        muzzleFlashEffect.SetActive(false);
    }
    private IEnumerator OnReload()
    {
        isReload = true;
        animator.OnReload();
        PlaySound(audioClipReload);
        while (true)
        {
            if(audioSource.isPlaying == false && (animator.CurrentAnimationIs("Movement") || animator.CurrentAnimationIs("AimFirePose")))
            {
                isReload = false;
                weaponSettings.currentAmmoPack --;
                onAmmoPackEvent.Invoke(weaponSettings.currentAmmoPack);
                weaponSettings.currentAmmo = weaponSettings.maxAmmo;
                onAmmoEvent.Invoke(weaponSettings.currentAmmo, weaponSettings.maxAmmo);

                yield break;
            }
            yield return null;
        }
    }
    private void TwoStepRaycast()
    {
        Ray ray;
        RaycastHit hit;
        Vector3 targetPoint = Vector3.zero;

        ray = mainCamera.ViewportPointToRay(Vector2.one * 0.5f);
        if(Physics.Raycast(ray, out hit, weaponSettings.attackDistance))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * weaponSettings.attackDistance;
        }
        Debug.DrawRay(ray.origin, ray.direction * weaponSettings.attackDistance, Color.red);
        Vector3 attackDirection = (targetPoint - bulletSpawnPoint.position).normalized;
        if(Physics.Raycast(bulletSpawnPoint.position, attackDirection, out hit, weaponSettings.attackDistance))
        {
            hitScanMemoryPool.SpawnHitScan(hit);
            if (hit.transform.CompareTag("HitScanEnemy"))
            {
                hit.transform.GetComponent<EnemyFSM>().TakeDamage(weaponSettings.damage);
            }else if (hit.transform.CompareTag("InteractionObject"))
            {
                hit.transform.GetComponent<InteractionObject>().TakeDamage(weaponSettings.damage);
            }
        }
        Debug.DrawRay(bulletSpawnPoint.position, attackDirection * weaponSettings.attackDistance, Color.blue);
    }
    private IEnumerator OnModeChange()
    {
        float current = 0;
        float percent = 0;
        float time = 0.35f;

        animator.AimModeIs = !animator.AimModeIs;
        imageAim.enabled = !imageAim.enabled;
        float start = mainCamera.fieldOfView;
        float end = animator.AimModeIs == true ? aimModeFOV : defaultModeFOV;

        isModeChange = true;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / time;

            mainCamera.fieldOfView = Mathf.Lerp(start, end, percent);
            yield return null;
        }
        isModeChange = false;
    }
    private void ResetVariables()
    {
        isReload = false;
        isAttack = false;
        isModeChange = false;
    }
    private void Recoil()
    {
        Vector3 recoilVector = new Vector3(Random.Range(-recoilKickback.x, recoilKickback.x), recoilKickback.y, recoilKickback.z);
        Vector3 recoilCamVector = new Vector3(-recoilVector.y * 400f, recoilVector.x * 200f, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition + recoilVector, recoilAmount / 100f);
        camRecoil.localRotation = Quaternion.Slerp(camRecoil.localRotation, Quaternion.Euler(camRecoil.localEulerAngles + recoilCamVector), recoilAmount);
    }
    private void RecoilBack()
    {
        camRecoil.localRotation = Quaternion.Slerp(camRecoil.localRotation, Quaternion.identity, Time.deltaTime * 2f);
    }
}
