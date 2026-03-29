using UnityEngine;

/// <summary>
/// 최종 데미지에 ±5% 랜덤 변동을 적용
/// </summary>
public class RandomVarianceModifier : IDamageModifier
{
    private const float VARIANCE_PERCENT = 5f;

    public int Priority => 400; // 모든 계산의 마지막에 적용

    public bool IsApplicable(DamageContext context)
    {
        return true;
    }

    public float ModifyDamage(float currentDamage, DamageContext context)
    {
        
        float minMultiplier = 1f - (VARIANCE_PERCENT / 100f);
        float maxMultiplier = 1f + (VARIANCE_PERCENT / 100f);
        float randomMultiplier = Random.Range(minMultiplier, maxMultiplier);

        return currentDamage * randomMultiplier;
    }
}
