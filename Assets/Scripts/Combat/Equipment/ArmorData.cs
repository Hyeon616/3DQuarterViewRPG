using UnityEngine;

/// <summary>
/// 개별 방어구 데이터
/// 기본 스탯 + 타입 배율 = 최종 스탯
/// </summary>
[CreateAssetMenu(fileName = "Armor_", menuName = "Combat/Equipment/Armor Data")]
public class ArmorData : ScriptableObject
{
    [Header("방어구 정보")]
    [SerializeField] private string armorName;
    [SerializeField] private Sprite icon;
    [SerializeField] private int requiredLevel = 1;

    [Header("방어구 타입")]
    [SerializeField] private ArmorTypeData armorTypeData;

    [Header("기본 스탯")]
    [SerializeField] private float baseHp = 100f;
    [SerializeField] private float baseDefense = 5f;

    [Header("레벨당 성장치")]
    [SerializeField] private float hpPerLevel = 10f;
    [SerializeField] private float defensePerLevel = 0.5f;

    // 기본 정보
    public string ArmorName => armorName;
    public Sprite Icon => icon;
    public int RequiredLevel => requiredLevel;
    public ArmorTypeData TypeData => armorTypeData;
    public ArmorType ArmorType => armorTypeData != null ? armorTypeData.ArmorType : ArmorType.None;

    // 기본 스탯
    public float BaseHp => baseHp;
    public float BaseDefense => baseDefense;
    public float HpPerLevel => hpPerLevel;
    public float DefensePerLevel => defensePerLevel;

    /// <summary>
    /// 레벨에 따른 최종 HP (기본 + 레벨 성장) * 타입 배율
    /// </summary>
    public float GetHp(int level)
    {
        float baseValue = baseHp + hpPerLevel * (level - 1);
        float multiplier = armorTypeData != null ? armorTypeData.HpMultiplier : 1f;
        return baseValue * multiplier;
    }

    /// <summary>
    /// 레벨에 따른 최종 방어력 (기본 + 레벨 성장) * 타입 배율
    /// </summary>
    public float GetDefense(int level)
    {
        float baseValue = baseDefense + defensePerLevel * (level - 1);
        float multiplier = armorTypeData != null ? armorTypeData.DefenseMultiplier : 1f;
        return baseValue * multiplier;
    }
}
