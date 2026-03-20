public class BaseStatModifier : IDamageModifier
{
    private const float DEFAULT_ATTACK = 10f;

    public int Priority => 0;

    public bool IsApplicable(DamageContext context)
    {
        return true;
    }

    public float ModifyDamage(float currentDamage, DamageContext context)
    {
        float attack = context.AttackerStats?.Attack ?? DEFAULT_ATTACK;
        return currentDamage + attack;
    }
}
