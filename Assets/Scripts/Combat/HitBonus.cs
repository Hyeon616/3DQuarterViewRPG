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

public enum DamageType
{
    Normal,
    Shield,
    Stagger
}

public struct HitBonusData
{
    public float DamageMultiplier;
    public float CriticalChanceBonus;
    public float StaggerDamageBonus;

    public HitBonusData(float damageMultiplier, float criticalChanceBonus, float staggerDamageBonus)
    {
        DamageMultiplier = damageMultiplier;
        CriticalChanceBonus = criticalChanceBonus;
        StaggerDamageBonus = staggerDamageBonus;
    }
}

public class HitBonus
{
    private readonly HitBonusData _noneBonus;
    private readonly HitBonusData _backBonus;
    private readonly HitBonusData _headBonus;

    public HitBonus(
        float backDamageMultiplier = 1.05f,
        float backCriticalChanceBonus = 0.1f,
        float headDamageMultiplier = 1.2f,
        float headStaggerDamageBonus = 0.1f)
    {
        _noneBonus = new HitBonusData(1f, 0f, 0f);
        _backBonus = new HitBonusData(backDamageMultiplier, backCriticalChanceBonus, 0f);
        _headBonus = new HitBonusData(headDamageMultiplier, 0f, headStaggerDamageBonus);
    }

    public bool IsBonusHit(AttackType attackType, HitDirection hitDirection)
    {
        return attackType switch
        {
            AttackType.Head => hitDirection == HitDirection.Head,
            AttackType.Back => hitDirection == HitDirection.Back,
            _ => false
        };
    }

    public HitBonusData GetBonus(HitDirection hitDirection)
    {
        return hitDirection switch
        {
            HitDirection.Back => _backBonus,
            HitDirection.Head => _headBonus,
            _ => _noneBonus
        };
    }

    public HitBonusData NoneBonus => _noneBonus;
}