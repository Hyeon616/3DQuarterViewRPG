using UnityEngine;

public struct DamageContext
{
    public GameObject Attacker;
    public IPlayerStat AttackerStats;
    public GameObject Target;
    public SkillData Skill;
    public float BaseDamage;
    public bool IsCritical;
    public HitDirection HitDirection;
    public AttackType AttackType;
    public HitBonusData HitBonus;

    public DamageContext(
        GameObject attacker,
        IPlayerStat attackerStats,
        GameObject target,
        SkillData skill,
        bool isCritical,
        HitDirection hitDirection,
        HitBonusData hitBonus)
    {
        Attacker = attacker;
        AttackerStats = attackerStats;
        Target = target;
        Skill = skill;
        BaseDamage = skill?.BaseDamage ?? 0f;
        IsCritical = isCritical;
        HitDirection = hitDirection;
        AttackType = skill?.AttackType ?? AttackType.Normal;
        HitBonus = hitBonus;
    }
}
