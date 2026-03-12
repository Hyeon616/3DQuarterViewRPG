public interface IPlayerStat
{
    float MaxHp { get; }
    float Attack { get; }
    float Defense { get; }
    float CriticalChance { get; }
    float CriticalDamage { get; }
    float DamageIncrease { get; }
    float AttackSpeed { get; }
    float CooldownReduction { get; }
    float ManaReduction { get; }
    int Level { get; }
}
