using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    private WeaponBase weapon;      // ���� ����
    [Header("Components")]
    [SerializeField]
    private PlayerStatus status;                    // �÷��̾��� ����

    [Header("Weapon Base")]
    [SerializeField]
    private TextMeshProUGUI textWeaponName;     // ���� �̸�
    [SerializeField]
    private Image imageWeaponIcon;              // ���� ������
    [SerializeField]
    private Sprite[] spriteWeaponIcons;         // ���� �������� ��������Ʈ �迭
    [SerializeField]
    private Vector2[] sizeWeaponIcons;          // ���� �������� ũ�� �迭

    [Header("Ammo")]
    [SerializeField]
    private TextMeshProUGUI textAmmo;           // ź�� ���

    [Header("AmmoPack")]
    [SerializeField]
    private GameObject ammoPackUIPrefab;    // źâ UI ������
    [SerializeField]
    private Transform ammoPackParent;       // źâ UI�� ��ġ�Ǵ� �г�
    [SerializeField]
    private int maxAmmoPackCount;

    private List<GameObject> ammoPackList;  // źâ UI ����Ʈ

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
