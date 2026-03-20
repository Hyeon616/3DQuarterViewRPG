public interface IDamageModifier
{
    int Priority { get; }
    float ModifyDamage(float currentDamage, DamageContext context);
    bool IsApplicable(DamageContext context);
}
