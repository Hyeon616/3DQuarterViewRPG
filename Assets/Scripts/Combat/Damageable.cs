using Mirror;
using UnityEngine;
using System;

public class Damageable : NetworkBehaviour, IDamageable
{
    [SerializeField] private MonsterData monsterData;

    [SyncVar(hook = nameof(HealthChanged))]
    private float _currentHealth;

    private float _maxHealth;

    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public bool IsAlive => _currentHealth > 0f;

    public event Action<float, float> OnHealthUpdated;
    public event Action<DamageEventData> OnDamageReceived;
    public event Action OnDeath;

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (monsterData != null)
        {
            _maxHealth = monsterData.MaxHealth;
        }

        _currentHealth = _maxHealth;
    }

    [Server]
    public void TakeDamage(float damage, HitBonusData hitBonus, GameObject attacker)
    {
        TakeDamage(damage, hitBonus, attacker, DamageType.Normal, false, AttackType.Normal, HitDirection.Normal);
    }

    [Server]
    public void TakeDamage(float damage, HitBonusData hitBonus, GameObject attacker, DamageType damageType, bool isCritical, AttackType attackType, HitDirection hitDirection)
    {
        if (!IsAlive) return;

        float finalDamage = damage * hitBonus.DamageMultiplier;
        _currentHealth = Mathf.Max(0f, _currentHealth - finalDamage);

        Vector3 hitPosition = transform.position + Vector3.up * 2f;
        DamageReceived(finalDamage, hitPosition, (int)damageType, isCritical, (int)attackType, (int)hitDirection);

        if (_currentHealth <= 0f)
        {
            SendDeath();
        }
    }

    [ClientRpc]
    private void DamageReceived(float damage, Vector3 position, int damageType, bool isCritical, int attackType, int hitDirection)
    {
        var eventData = new DamageEventData(
            damage,
            position,
            (DamageType)damageType,
            isCritical,
            (AttackType)attackType,
            (HitDirection)hitDirection
        );
        OnDamageReceived?.Invoke(eventData);
    }

    [Server]
    private void SendDeath()
    {
        OnDeath?.Invoke();
        Death();
    }

    [ClientRpc]
    private void Death()
    {
        OnDeath?.Invoke();
    }

    private void HealthChanged(float oldHealth, float newHealth)
    {
        OnHealthUpdated?.Invoke(newHealth, _maxHealth);
    }
}