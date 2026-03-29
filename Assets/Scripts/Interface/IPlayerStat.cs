public interface IPlayerStat
{
    float MaxHp { get; }
    float Attack { get; }
    float Defense { get; }
    float CriticalChance { get; }
    /// <summary>
    /// 크리티컬 추가 데미지 (퍼센트)
    /// 100 = 100% 추가 = 2배 데미지
    /// 150 = 150% 추가 = 2.5배 데미지
    /// </summary>
    float CriticalDamage { get; }
    float DamageIncrease { get; }
    float AttackSpeed { get; }
    float CooldownReduction { get; }
    float ManaReduction { get; }
    int Level { get; }
}
