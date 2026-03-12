using UnityEngine;

public enum StatType
{
    CriticalChance,     // 치명타 확률
    CriticalDamage,     // 치명타 피해
    DamageIncrease,     // 데미지 증가
    AttackSpeed,        // 공격 속도
    CooldownReduction,  // 쿨타임 감소
    ManaReduction       // 마나 소모량 감소
}

[System.Serializable]
public struct StatModifier
{
    [SerializeField] private StatType statType;
    [SerializeField] private float value;

    public StatType StatType => statType;
    public float Value => value;

    public StatModifier(StatType type, float value)
    {
        this.statType = type;
        this.value = value;
    }
}
