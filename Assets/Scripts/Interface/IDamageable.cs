using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, HitBonus hitBonus, GameObject attacker);
    bool IsAlive { get; }
}