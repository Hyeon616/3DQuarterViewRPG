using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, HitBonusData hitBonus, GameObject attacker);
    bool IsAlive { get; }
}