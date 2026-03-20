public class HitBonusModifier : IDamageModifier
{
    public int Priority => 300;

    public bool IsApplicable(DamageContext context)
    {
        return true;
    }

    public float ModifyDamage(float currentDamage, DamageContext context)
    {
        return currentDamage * context.HitBonus.DamageMultiplier;
    }
}
