using UnityEngine;

public struct DamageEventData
{
    public float Damage;
    public Vector3 Position;
    public DamageType DamageType;
    public AttackType AttackType;
    public HitDirection HitDirection;

    public DamageEventData(float damage, Vector3 position, DamageType damageType, AttackType attackType, HitDirection hitDirection)
    {
        Damage = damage;
        Position = position;
        DamageType = damageType;
        AttackType = attackType;
        HitDirection = hitDirection;
    }
}