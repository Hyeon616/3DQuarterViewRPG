using UnityEngine;

/// <summary>
/// 소비 아이템 데이터
/// </summary>
[CreateAssetMenu(fileName = "Consumable_", menuName = "Items/Consumable Item")]
public class ConsumableItemData : ItemData
{
    [Header("소비 효과")]
    [SerializeField] private ConsumableEffect[] effects;
    [SerializeField] private float cooldown = 0f; // 쿨다운 (초)

    public ConsumableEffect[] Effects => effects;
    public float Cooldown => cooldown;

    public override bool IsUsable => true;

    public override string GetTooltipInfo()
    {
        string info = base.GetTooltipInfo();
        if (effects != null && effects.Length > 0)
        {
            info += "\n\n<color=green>효과:</color>";
            foreach (var effect in effects)
            {
                info += $"\n- {effect.GetDescription()}";
            }
        }
        if (cooldown > 0)
        {
            info += $"\n\n<color=yellow>쿨다운: {cooldown}초</color>";
        }
        return info;
    }
}

/// <summary>
/// 소비 효과 타입
/// </summary>
[System.Serializable]
public class ConsumableEffect
{
    public enum EffectType
    {
        RestoreHP,
        RestoreMana,
        Buff,
        Debuff
    }

    public EffectType effectType;
    public float value;
    public float duration; // Buff/Debuff용

    public string GetDescription()
    {
        switch (effectType)
        {
            case EffectType.RestoreHP:
                return $"HP {value} 회복";
            case EffectType.RestoreMana:
                return $"마나 {value} 회복";
            case EffectType.Buff:
                return $"버프 효과 ({duration}초)";
            case EffectType.Debuff:
                return $"디버프 효과 ({duration}초)";
            default:
                return "알 수 없는 효과";
        }
    }
}
