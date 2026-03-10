using UnityEngine;

public struct DamageEventData
{
    public float Damage;
    public Vector3 Position;
    public DamageType DamageType;
    public bool IsCritical;
    public AttackType AttackType;
    public HitDirection HitDirection;

    public DamageEventData(float damage, Vector3 position, DamageType damageType, bool isCritical, AttackType attackType, HitDirection hitDirection)
    {
        Damage = damage;
        Position = position;
        DamageType = damageType;
        IsCritical = isCritical;
        AttackType = attackType;
        HitDirection = hitDirection;
    }
}