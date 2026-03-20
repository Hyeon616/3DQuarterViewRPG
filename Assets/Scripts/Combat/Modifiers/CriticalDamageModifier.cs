public class CriticalDamageModifier : IDamageModifier
{
    private const float DEFAULT_CRITICAL_DAMAGE = 150f;

    public int Priority => 100;

    public bool IsApplicable(DamageContext context)
    {
        return context.IsCritical;
    }

    public float ModifyDamage(float currentDamage, DamageContext context)
    {
        float critDamage = context.AttackerStats?.CriticalDamage ?? DEFAULT_CRITICAL_DAMAGE;
        return currentDamage * (1f + critDamage / 100f);
    }
}
