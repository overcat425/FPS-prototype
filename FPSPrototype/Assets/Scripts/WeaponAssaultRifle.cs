using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponAssaultRifle : WeaponBase
{
    [Header("Fire effects")]
    [SerializeField]
    private GameObject muzzleFlashEffect;   // �ѱ� ȭ�� ����Ʈ

    [Header("Spawn Points")]
    [SerializeField]
    private Transform casingSpawnPoint;     // ź�� ������ġ
    [SerializeField]
    private Transform bulletSpawnPoint;     // �Ѿ� ������ġ

    [Header("Aim UI")]
    [SerializeField]
    private Image imageAim;                     // ���� ��忡 ���� �����̹��� ����

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipTakeOutWeapon;   // ���� �����¼Ҹ�
    [SerializeField]
    private AudioClip audioClipFire;                    // ���� ����
    [SerializeField]
    private AudioClip audioClipReload;              // ������ ����

    private bool isModeChange = false;          // ���Ӹ��?
    private float defaultModeFOV = 60;          // �⺻��� FOV
    private float aimModeFOV = 30;              // ���Ӹ�� FOV

    private CasingMemoryPool casingMemoryPool;  // ź�� ���� �� ����
    private HitScanMemoryPool hitScanMemoryPool;// ���� ����Ʈ ���� �� ����
    private Camera mainCamera;                  // �Ѿ� ���� �߻�

    public Transform camRecoil;                 // �ݵ��� ���� ķ ������
    public Vector3 recoilKickback;              // �ݵ� ȸ���� ���� ���Ͱ�
    public float recoilAmount;                  // �ݵ� ���� ����
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
