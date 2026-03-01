using Mirror;
using UnityEngine;
using System;

public class Damageable : NetworkBehaviour, IDamageable
{
    [SerializeField] private MonsterData monsterData;

    [SyncVar(hook = nameof(OnHealthChanged))]
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
        TakeDamage(damage, hitBonus, attacker, DamageType.Normal, AttackType.Normal, HitDirection.Normal);
    }

    [Server]
    public void TakeDamage(float damage, HitBonusData hitBonus, GameObject attacker, DamageType damageType, AttackType attackType, HitDirection hitDirection)
    {
        if (!IsAlive) return;

        float finalDamage = damage * hitBonus.DamageMultiplier;
        _currentHealth = Mathf.Max(0f, _currentHealth - finalDamage);

        Vector3 hitPosition = transform.position + Vector3.up * 2f;
        RpcOnDamageReceived(finalDamage, hitPosition, (int)damageType, (int)attackType, (int)hitDirection);

        if (_currentHealth <= 0f)
        {
            HandleDeath();
        }
    }

    [ClientRpc]
    private void RpcOnDamageReceived(float damage, Vector3 position, int damageType, int attackType, int hitDirection)
    {
        var eventData = new DamageEventData(
            damage,
            position,
            (DamageType)damageType,
            (AttackType)attackType,
            (HitDirection)hitDirection
        );
        OnDamageReceived?.Invoke(eventData);
    }

    [Server]
    private void HandleDeath()
    {
        OnDeath?.Invoke();
        RpcOnDeath();
    }

    [ClientRpc]
    private void RpcOnDeath()
    {
        OnDeath?.Invoke();
    }

    private void OnHealthChanged(float oldHealth, float newHealth)
    {
        OnHealthUpdated?.Invoke(newHealth, _maxHealth);
    }
}