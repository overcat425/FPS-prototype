using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRevolver : WeaponBase
{
    [Header("Fire Effects")]
    [SerializeField]
    private GameObject muzzleFlashEffect;

    [Header("Spawn Points")]
    [SerializeField]
    private Transform bulletSpawnPoint;

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipFire;
    [SerializeField]
    private AudioClip audioClipReload;

    private HitScanMemoryPool hitScanMemoryPool;
    private Camera mainCamera;

    public Transform camRecoil;
    public Vector3 recoilKickback;
    public float recoilAmount;
    private void OnEnable()
    {
        muzzleFlashEffect.SetActive(false);
        onAmmoPackEvent.Invoke(weaponSettings.currentAmmoPack);
        onAmmoEvent.Invoke(weaponSettings.currentAmmo, weaponSettings.maxAmmo);
        ResetVariables();
    }
    private void Awake()
    {
        base.Setup();
        hitScanMemoryPool = GetComponent<HitScanMemoryPool>();
        mainCamera = Camera.main;
        weaponSettings.currentAmmoPack = weaponSettings.maxAmmoPack;
        weaponSettings.currentAmmo = weaponSettings.maxAmmo;
    }
    private void Update()
    {
        RecoilBack();
    }
    public override void StartWeapon(int type = 0)
    {
        if(type == 0 && isAttack == false && isReload == false)
        {
            OnAttack();
        }
    }
    public override void StopWeapon(int type = 0)
    {
        isAttack = false;
    }
    public override void StartReload()
    {
        if (isReload == true || weaponSettings.currentAmmoPack <= 0) return;
        StopWeapon();
        StartCoroutine("OnReload");
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
            weaponSettings.currentAmmo--;
            onAmmoEvent.Invoke(weaponSettings.currentAmmo, weaponSettings.maxAmmo);
            animator.Play("Fire", -1, 0);
            StartCoroutine("OnMuzzleFlashEffect");
            PlaySound(audioClipFire);
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
            if (audioSource.isPlaying == false && (animator.CurrentAnimationIs("Movement") || animator.CurrentAnimationIs("AimFirePose")))
            {
                isReload = false;
                weaponSettings.currentAmmoPack--;
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
        if (Physics.Raycast(ray, out hit, weaponSettings.attackDistance))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * weaponSettings.attackDistance;
        }
        Debug.DrawRay(ray.origin, ray.direction * weaponSettings.attackDistance, Color.red);
        Vector3 attackDirection = (targetPoint - bulletSpawnPoint.position).normalized;
        if (Physics.Raycast(bulletSpawnPoint.position, attackDirection, out hit, weaponSettings.attackDistance))
        {
            hitScanMemoryPool.SpawnHitScan(hit);
            if (hit.transform.CompareTag("HitScanEnemy"))
            {
                hit.transform.GetComponent<EnemyFSM>().TakeDamage(weaponSettings.damage);
            }
            else if (hit.transform.CompareTag("InteractionObject"))
            {
                hit.transform.GetComponent<InteractionObject>().TakeDamage(weaponSettings.damage);
            }
        }
        Debug.DrawRay(bulletSpawnPoint.position, attackDirection * weaponSettings.attackDistance, Color.blue);
    }
    private void ResetVariables()
    {
        isReload = false;
        isAttack = false;
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
