using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    private WeaponBase weapon;      // 현재 무기
    [Header("Components")]
    [SerializeField]
    private PlayerStatus status;                    // 플레이어의 상태

    [Header("Weapon Base")]
    [SerializeField]
    private TextMeshProUGUI textWeaponName;     // 무기 이름
    [SerializeField]
    private Image imageWeaponIcon;              // 무기 아이콘
    [SerializeField]
    private Sprite[] spriteWeaponIcons;         // 무기 아이콘의 스프라이트 배열
    [SerializeField]
    private Vector2[] sizeWeaponIcons;          // 무기 아이콘의 크기 배열

    [Header("Ammo")]
    [SerializeField]
    private TextMeshProUGUI textAmmo;           // 탄약 출력

    [Header("AmmoPack")]
    [SerializeField]
    private GameObject ammoPackUIPrefab;    // 탄창 UI 프리팹
    [SerializeField]
    private Transform ammoPackParent;       // 탄창 UI가 배치되는 패널
    [SerializeField]
    private int maxAmmoPackCount;

    private List<GameObject> ammoPackList;  // 탄창 UI 리스트

    [Header("HP & BloodScreen UI")]
    [SerializeField]
    private TextMeshProUGUI textHP;
    [SerializeField]
    private Image imageBloodScreen;
    [SerializeField]
    private AnimationCurve curveBloodScreen;

    private void Awake()
    {
        status.onHPEvent.AddListener(UpdateHPHUD);
    }
    public void SetupAllWeapons(WeaponBase[] weapons)
    {
        SetupAmmoPack();
        for(int i = 0; i < weapons.Length; ++i)
        {
            weapons[i].onAmmoEvent.AddListener(UpdateAmmoHUD);
            weapons[i].onAmmoPackEvent.AddListener(UpdateAmmoPackHUD);
        }
    }
    public void SwitchngWeapon(WeaponBase newWeapon)
    {
        weapon = newWeapon;
        SetupWeapon();
    }
    private void SetupWeapon()
    {
        textWeaponName.text = weapon.WeaponName.ToString();
        imageWeaponIcon.sprite = spriteWeaponIcons[(int)weapon.WeaponName];
        imageWeaponIcon.rectTransform.sizeDelta = sizeWeaponIcons[(int)weapon.WeaponName];
    }
    private void UpdateAmmoHUD(int currentAmmo, int maxAmmo)
    {
        textAmmo.text = $"<size=60>{currentAmmo}/</size>{maxAmmo}";
    }
    private void SetupAmmoPack()
    {
        ammoPackList = new List<GameObject>();
        for(int i = 0; i<maxAmmoPackCount; ++i)
        {
            GameObject clone = Instantiate(ammoPackUIPrefab);
            clone.transform.SetParent(ammoPackParent);
            clone.SetActive(false);
            ammoPackList.Add(clone);
        }
    }
    private void UpdateAmmoPackHUD(int currentAmmoPack)
    {
        for(int i = 0; i<ammoPackList.Count; ++i)
        {
            ammoPackList[i].SetActive(false);
        }
        for(int i = 0; i<currentAmmoPack; ++i)
        {
            ammoPackList[i].SetActive(true);
        }
    }
    private void UpdateHPHUD(int previous, int current)
    {
        textHP.text = "HP " + current;
        if (previous <= current) return;
        if(previous - current > 0)
        {
            StopCoroutine("OnBloodScreen");
            StartCoroutine("OnBloodScreen");
        }
    }
    private IEnumerator OnBloodScreen()
    {
        float percent = 0;
        while(percent < 1)
        {
            percent += Time.deltaTime;

            Color color = imageBloodScreen.color;
            color.a = Mathf.Lerp(1, 0, curveBloodScreen.Evaluate(percent));
            imageBloodScreen.color = color;

            yield return null;
        }
    }
}
