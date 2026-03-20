using UnityEngine;

/// <summary>
/// 개별 무기 데이터
/// 기본 스탯 + 타입 배율 = 최종 스탯
/// </summary>
[CreateAssetMenu(fileName = "Weapon_", menuName = "Combat/Equipment/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("무기 정보")]
    [SerializeField] private string weaponName;
    [SerializeField] private Sprite icon;
    [SerializeField] private int requiredLevel = 1;

    [Header("무기 타입")]
    [SerializeField] private WeaponTypeData weaponTypeData;

    [Header("기본 스탯")]
    [SerializeField] private float baseAttack = 10f;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float baseCriticalChance = 0.05f;
    [SerializeField] private float baseCriticalDamage = 1.5f;

    [Header("레벨당 성장치")]
    [SerializeField] private float attackPerLevel = 1f;

    // 기본 정보
    public string WeaponName => weaponName;
    public Sprite Icon => icon;
    public int RequiredLevel => requiredLevel;
    public WeaponTypeData TypeData => weaponTypeData;
    public WeaponType WeaponType => weaponTypeData != null ? weaponTypeData.WeaponType : WeaponType.None;

    // 기본 스탯
    public float BaseAttack => baseAttack;
    public float BaseAttackSpeed => baseAttackSpeed;
    public float BaseCriticalChance => baseCriticalChance;
    public float BaseCriticalDamage => baseCriticalDamage;
    public float AttackPerLevel => attackPerLevel;

    /// <summary>
    /// 레벨에 따른 최종 공격력 (기본 + 레벨 성장) * 타입 배율
    /// </summary>
    public float GetAttack(int level)
    {
        float baseValue = baseAttack + attackPerLevel * (level - 1);
        float multiplier = weaponTypeData != null ? weaponTypeData.AttackMultiplier : 1f;
        return baseValue * multiplier;
    }

    /// <summary>
    /// 최종 공격 속도 (기본 * 타입 배율)
    /// </summary>
    public float GetAttackSpeed()
    {
        float multiplier = weaponTypeData != null ? weaponTypeData.AttackSpeedMultiplier : 1f;
        return baseAttackSpeed * multiplier;
    }

    /// <summary>
    /// 최종 치명타 확률 (기본 * 타입 배율)
    /// </summary>
    public float GetCriticalChance()
    {
        float multiplier = weaponTypeData != null ? weaponTypeData.CriticalChanceMultiplier : 1f;
        return baseCriticalChance * multiplier;
    }

    /// <summary>
    /// 최종 치명타 데미지 (기본 * 타입 배율)
    /// </summary>
    public float GetCriticalDamage()
    {
        float multiplier = weaponTypeData != null ? weaponTypeData.CriticalDamageMultiplier : 1f;
        return baseCriticalDamage * multiplier;
    }
}
