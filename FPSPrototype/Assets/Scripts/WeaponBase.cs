using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType {  Main = 0, Sub, Melee, Throw }
[System.Serializable]
public class AmmoEvent : UnityEngine.Events.UnityEvent<int, int> { }
[System.Serializable]
public class AmmoPackEvent : UnityEngine.Events.UnityEvent<int> { }
public abstract class WeaponBase : MonoBehaviour
{
    [Header("WeaponBase")]
    [SerializeField]
    protected WeaponType weaponType;                    // 무기 종류
    [SerializeField]
    protected WeaponSettings weaponSettings;            // 무기 설정
  
    protected float lastAttackTime = 0;
    protected bool isReload = false;                        // 재장전중인가?
    protected bool isAttack = false;                        // 공격중인가?
    protected AudioSource audioSource;                 // 사운드 재생 컴포넌트
    protected PlayerAnimator animator;                  // 애니메이션 재생 제어

    [HideInInspector]
    public AmmoEvent onAmmoEvent = new AmmoEvent();
    [HideInInspector]
    public AmmoPackEvent onAmmoPackEvent = new AmmoPackEvent();

    public PlayerAnimator Animator => animator;
    public WeaponName WeaponName => weaponSettings.weaponName;
    public int CurrentAmmoPack => weaponSettings.currentAmmoPack;
    public int MaxAmmoPack => weaponSettings.maxAmmoPack;

    public abstract void StartWeapon(int type = 0);
    public abstract void StopWeapon(int type = 0);
    public abstract void StartReload();

    public void PlaySound(AudioClip clip)                       // 사운드 재생
    {
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
    protected void Setup()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<PlayerAnimator>();
    }
    public virtual void IncreaseAmmoPack(int AmmoPack)
    {
        weaponSettings.currentAmmoPack = CurrentAmmoPack + AmmoPack > MaxAmmoPack ? MaxAmmoPack : CurrentAmmoPack + AmmoPack;
        onAmmoPackEvent.Invoke(CurrentAmmoPack);
    }
}
