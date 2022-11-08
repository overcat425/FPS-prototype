    public enum WeaponName { AssaultRifle = 0, Revolver, CombatKnife, Grenade }

    [System.Serializable]
    public struct WeaponSettings
    {
        public WeaponName weaponName;
        public int damage;
        public int currentAmmoPack;
        public int maxAmmoPack;
        public int currentAmmo;
        public int maxAmmo;
        public float attackDistance;
        public float attackRate;
        public bool isAutomaticAttack;
    }
