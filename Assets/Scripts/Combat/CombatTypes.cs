public enum HitDirection
{
    Normal,
    Head,
    Back
}

public enum AttackType
{
    Normal,
    Head,
    Back
}

public struct HitBonus
{
    public float DamageMultiplier;
    public float CriticalChanceBonus;
    public float StaggerDamageBonus;

    public HitBonus(float damageMultiplier, float criticalChanceBonus, float staggerDamageBonus)
    {
        DamageMultiplier = damageMultiplier;
        CriticalChanceBonus = criticalChanceBonus;
        StaggerDamageBonus = staggerDamageBonus;
    }
}