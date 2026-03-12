using Mirror;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerController))]
public class PlayerStatController : NetworkBehaviour, IPlayerStat
{
    [Header("직업 스탯")]
    [SerializeField] private CombatStat combatStat;

    [SyncVar]
    private int _level = 1;

    private PlayerStatAllocation _allocation;

    // 기본 스탯
    private float _maxHp;
    private float _attack;
    private float _defense;
    private float _criticalChance;
    private float _criticalDamage;

    // 투자 스탯
    private float _damageIncrease;
    private float _attackSpeed;
    private float _cooldownReduction;
    private float _manaReduction;

    public float MaxHp => _maxHp;
    public float Attack => _attack;
    public float Defense => _defense;
    public float CriticalChance => _criticalChance;
    public float CriticalDamage => _criticalDamage;
    public float DamageIncrease => _damageIncrease;
    public float AttackSpeed => _attackSpeed;
    public float CooldownReduction => _cooldownReduction;
    public float ManaReduction => _manaReduction;
    public int Level => _level;
    public CombatStat CombatStat => combatStat;

    public override void OnStartServer()
    {
        base.OnStartServer();
        _allocation = GetComponent<PlayerStatAllocation>();
        if (_allocation != null)
        {
            _allocation.OnAllocationChanged += RecalculateStats;
        }
        RecalculateStats();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (_allocation != null)
        {
            _allocation.OnAllocationChanged -= RecalculateStats;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        _allocation = GetComponent<PlayerStatAllocation>();
        if (_allocation != null)
        {
            _allocation.OnAllocationChanged += RecalculateStats;
        }
        RecalculateStats();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (_allocation != null)
        {
            _allocation.OnAllocationChanged -= RecalculateStats;
        }
    }

    [Server]
    public void SetLevel(int level)
    {
        int oldLevel = _level;
        _level = Mathf.Max(1, level);

        // 레벨업 시 포인트 지급
        if (_allocation != null && _level > oldLevel)
        {
            int pointsToAdd = _level - oldLevel;
            _allocation.AddPoints(pointsToAdd);
        }

        RecalculateStats();
        RpcRecalculateStats();
    }

    /// <summary>
    /// 테스트용 포인트 직접 추가
    /// </summary>
    [Server]
    public void AddStatPoints(int points)
    {
        _allocation?.AddPoints(points);
    }

    [ClientRpc]
    private void RpcRecalculateStats()
    {
        RecalculateStats();
    }

    private void RecalculateStats()
    {
        if (combatStat == null) return;

        // 기본 스탯 (직업 + 레벨)
        _maxHp = combatStat.GetHp(_level);
        _attack = combatStat.GetAttack(_level);
        _defense = combatStat.GetDefense(_level);
        _criticalChance = combatStat.GetCriticalChance();
        _criticalDamage = combatStat.GetCriticalDamage();

        // 투자 스탯 초기화
        _damageIncrease = 0f;
        _attackSpeed = 0f;
        _cooldownReduction = 0f;
        _manaReduction = 0f;

        // 투자 효과 적용
        if (_allocation != null)
        {
            var modifiers = _allocation.GetTotalModifiers();
            ApplyModifiers(modifiers);
        }
    }

    private void ApplyModifiers(Dictionary<StatType, float> modifiers)
    {
        foreach (var mod in modifiers)
        {
            switch (mod.Key)
            {
                case StatType.CriticalChance:
                    _criticalChance += mod.Value;
                    break;
                case StatType.CriticalDamage:
                    _criticalDamage += mod.Value;
                    break;
                case StatType.DamageIncrease:
                    _damageIncrease += mod.Value;
                    break;
                case StatType.AttackSpeed:
                    _attackSpeed += mod.Value;
                    break;
                case StatType.CooldownReduction:
                    _cooldownReduction += mod.Value;
                    break;
                case StatType.ManaReduction:
                    _manaReduction += mod.Value;
                    break;
            }
        }
    }
}
