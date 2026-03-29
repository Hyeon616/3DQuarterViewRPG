/// <summary>
/// 크리티컬 데미지 모디파이어
/// CriticalDamage는 "추가 데미지 퍼센트"로 정의
/// - 100 = 100% 추가 = 2배 데미지 (기본)
/// - 150 = 150% 추가 = 2.5배 데미지
/// - 200 = 200% 추가 = 3배 데미지
/// </summary>
public class CriticalDamageModifier : IDamageModifier
{
    private const float DEFAULT_CRITICAL_DAMAGE = 100f;

    public int Priority => 100;

    public bool IsApplicable(DamageContext context)
    {
        return context.IsCritical;
    }

    public float ModifyDamage(float currentDamage, DamageContext context)
    {
        float critDamage = context.AttackerStats?.CriticalDamage ?? DEFAULT_CRITICAL_DAMAGE;

        // 호환성: 값이 10 미만이면 배율로 간주 (1.5 = 1.5배 -> 50% 추가)
        if (critDamage < 10f)
        {
            critDamage = (critDamage - 1f) * 100f;
        }

        return currentDamage * (1f + critDamage / 100f);
    }
}
