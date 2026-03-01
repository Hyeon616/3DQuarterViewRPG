public class DamageResolver
{
    private readonly string _backAttackText;
    private readonly string _headAttackText;

    public DamageResolver(string backAttackText = "백어택", string headAttackText = "헤드어택")
    {
        _backAttackText = backAttackText;
        _headAttackText = headAttackText;
    }

    public bool IsBonusHit(AttackType attackType, HitDirection hitDirection)
    {
        return attackType switch
        {
            AttackType.Back => hitDirection == HitDirection.Back,
            AttackType.Head => hitDirection == HitDirection.Head,
            _ => false
        };
    }

    public string GetBonusText(HitDirection hitDirection)
    {
        return hitDirection switch
        {
            HitDirection.Back => _backAttackText,
            HitDirection.Head => _headAttackText,
            _ => ""
        };
    }

    public float CalculateFinalDamage(float baseDamage, HitBonusData hitBonus)
    {
        return baseDamage * hitBonus.DamageMultiplier;
    }

    public bool IsCriticalHit(float criticalChance)
    {
        return UnityEngine.Random.value < criticalChance;
    }

    public DamageType ResolveDamageType(bool isCritical, bool isShield, bool isStagger)
    {
        if (isStagger) return DamageType.Stagger;
        if (isShield) return DamageType.Shield;
        if (isCritical) return DamageType.Critical;
        return DamageType.Normal;
    }
}