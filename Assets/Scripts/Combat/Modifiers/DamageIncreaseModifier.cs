public class DamageIncreaseModifier : IDamageModifier
{
    public int Priority => 50;

    public bool IsApplicable(DamageContext context)
    {
        return true;
    }

    public float ModifyDamage(float currentDamage, DamageContext context)
    {
        float damageIncrease = context.AttackerStats?.DamageIncrease ?? 0f;
        return currentDamage * (1f + damageIncrease / 100f);
    }
}
