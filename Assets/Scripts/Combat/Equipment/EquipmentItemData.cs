using UnityEngine;

/// <summary>
/// 장비 아이템 (무기/방어구 통합)
/// 이것 하나만 만들면 끝!
/// </summary>
[CreateAssetMenu(fileName = "Equipment_", menuName = "Items/Equipment")]
public class EquipmentItemData : ItemData
{
    [Header("장비 종류")]
    [SerializeField] private EquipmentType equipmentType; // Weapon or Armor
    [SerializeField] private WeaponType weaponType; // equipmentType이 Weapon일 때
    [SerializeField] private ArmorType armorType; // equipmentType이 Armor일 때

    [Header("기본 스탯")]
    [SerializeField] private float baseAttack = 0f; // 무기만
    [SerializeField] private float baseAttackSpeed = 1f; // 무기만
    [SerializeField] private float baseCriticalChance = 0f; // 무기만
    [SerializeField] private float baseCriticalDamage = 2.0f; // 무기만 (2.0 = 100% 추가)

    [SerializeField] private float baseHp = 0f; // 방어구만
    [SerializeField] private float baseDefense = 0f; // 방어구만

    [Header("레벨 성장")]
    [SerializeField] private float attackPerLevel = 2f; // 무기만
    [SerializeField] private float hpPerLevel = 10f; // 방어구만
    [SerializeField] private float defensePerLevel = 1f; // 방어구만

    [Header("시각 효과")]
    [SerializeField] private GameObject weaponPrefab; // 무기 모델
    [SerializeField] private GameObject armorPrefab; // 방어구 모델

    // Properties
    public EquipmentType EquipmentType => equipmentType;
    public WeaponType WeaponType => weaponType;
    public ArmorType ArmorType => armorType;

    public float BaseAttack => baseAttack;
    public float BaseAttackSpeed => baseAttackSpeed;
    public float BaseCriticalChance => baseCriticalChance;
    public float BaseCriticalDamage => baseCriticalDamage;

    public float BaseHp => baseHp;
    public float BaseDefense => baseDefense;

    public GameObject WeaponPrefab => weaponPrefab;
    public GameObject ArmorPrefab => armorPrefab;

    public override bool IsEquippable => true;

    /// <summary>
    /// 레벨 적용된 공격력
    /// </summary>
    public float GetAttack(int level)
    {
        if (equipmentType != EquipmentType.Weapon) return 0f;

        float attack = baseAttack + attackPerLevel * (level - 1);

        // 타입별 배율 적용
        var typeData = EquipmentDatabase.Instance?.GetWeaponTypeData(weaponType);
        if (typeData != null)
        {
            attack *= typeData.AttackMultiplier;
        }

        return attack;
    }

    /// <summary>
    /// 공격 속도
    /// </summary>
    public float GetAttackSpeed()
    {
        if (equipmentType != EquipmentType.Weapon) return 1f;

        float attackSpeed = baseAttackSpeed;

        // 타입별 배율 적용
        var typeData = EquipmentDatabase.Instance?.GetWeaponTypeData(weaponType);
        if (typeData != null)
        {
            attackSpeed *= typeData.AttackSpeedMultiplier;
        }

        return attackSpeed;
    }

    /// <summary>
    /// 치명타 확률
    /// </summary>
    public float GetCriticalChance()
    {
        if (equipmentType != EquipmentType.Weapon) return 0f;
        return baseCriticalChance;
    }

    /// <summary>
    /// 치명타 데미지
    /// </summary>
    public float GetCriticalDamage()
    {
        if (equipmentType != EquipmentType.Weapon) return 2.0f;
        return baseCriticalDamage;
    }

    /// <summary>
    /// 레벨 적용된 HP
    /// </summary>
    public float GetHp(int level)
    {
        if (equipmentType != EquipmentType.Armor) return 0f;

        float hp = baseHp + hpPerLevel * (level - 1);

        // 타입별 배율 적용
        var typeData = EquipmentDatabase.Instance?.GetArmorTypeData(armorType);
        if (typeData != null)
        {
            hp *= typeData.HpMultiplier;
        }

        return hp;
    }

    /// <summary>
    /// 레벨 적용된 방어력
    /// </summary>
    public float GetDefense(int level)
    {
        if (equipmentType != EquipmentType.Armor) return 0f;

        float defense = baseDefense + defensePerLevel * (level - 1);

        // 타입별 배율 적용
        var typeData = EquipmentDatabase.Instance?.GetArmorTypeData(armorType);
        if (typeData != null)
        {
            defense *= typeData.DefenseMultiplier;
        }

        return defense;
    }

    public override string GetTooltipInfo()
    {
        string info = base.GetTooltipInfo();

        if (equipmentType == EquipmentType.Weapon)
        {
            info += $"\n\n<color=orange>무기 타입:</color> {weaponType}";
            info += $"\n<color=red>공격력:</color> {baseAttack} (+{attackPerLevel}/레벨)";
            info += $"\n<color=yellow>공격 속도:</color> {baseAttackSpeed}";
            info += $"\n<color=cyan>치명타 확률:</color> {baseCriticalChance}%";
            info += $"\n<color=magenta>치명타 데미지:</color> {(baseCriticalDamage - 1f) * 100f}%";
        }
        else if (equipmentType == EquipmentType.Armor)
        {
            info += $"\n\n<color=orange>방어구 타입:</color> {armorType}";
            info += $"\n<color=green>HP:</color> {baseHp} (+{hpPerLevel}/레벨)";
            info += $"\n<color=blue>방어력:</color> {baseDefense} (+{defensePerLevel}/레벨)";
        }

        return info;
    }
}

public enum EquipmentType
{
    Weapon,
    Armor
}
