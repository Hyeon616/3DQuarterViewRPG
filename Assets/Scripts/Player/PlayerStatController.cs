using Mirror;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어 스탯 관리
/// 장비 + StatTree 투자 기반 스탯 계산
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerStatController : NetworkBehaviour, IPlayerStat
{
    [SyncVar]
    private int _level = 1;

    private PlayerStatAllocation _allocation;
    private PlayerEquipment _equipment;

    // 기본 스탯 (장비 + 레벨)
    private float _maxHp;
    private float _attack;
    private float _defense;
    private float _criticalChance;
    private float _criticalDamage;

    // 투자 스탯 (StatTree)
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
    public PlayerEquipment Equipment => _equipment;

    /// <summary>
    /// 스탯이 변경되었을 때 발생하는 이벤트
    /// </summary>
    public event System.Action OnStatsChanged;

    public override void OnStartServer()
    {
        base.OnStartServer();

        _allocation = GetComponent<PlayerStatAllocation>();
        _equipment = GetComponent<PlayerEquipment>();

        if (_allocation != null)
            _allocation.OnAllocationChanged += RecalculateStats;
        if (_equipment != null)
            _equipment.OnEquipmentChanged += RecalculateStats;

        RecalculateStats();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (_allocation != null)
            _allocation.OnAllocationChanged -= RecalculateStats;
        if (_equipment != null)
            _equipment.OnEquipmentChanged -= RecalculateStats;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _allocation = GetComponent<PlayerStatAllocation>();
        _equipment = GetComponent<PlayerEquipment>();

        if (_allocation != null)
            _allocation.OnAllocationChanged += RecalculateStats;
        if (_equipment != null)
            _equipment.OnEquipmentChanged += RecalculateStats;

        RecalculateStats();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (_allocation != null)
            _allocation.OnAllocationChanged -= RecalculateStats;
        if (_equipment != null)
            _equipment.OnEquipmentChanged -= RecalculateStats;
    }

    [Server]
    public void SetLevel(int level)
    {
        int oldLevel = _level;
        _level = Mathf.Max(1, level);

        if (_allocation != null && _level > oldLevel)
        {
            int pointsToAdd = _level - oldLevel;
            _allocation.AddPoints(pointsToAdd);
        }

        RecalculateStats();
        RpcRecalculateStats();
    }

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
        // 장비 기반 스탯
        if (_equipment != null)
        {
            _maxHp = _equipment.GetHp(_level);
            _attack = _equipment.GetAttack(_level);
            _defense = _equipment.GetDefense(_level);
            _criticalChance = _equipment.GetCriticalChance();
            _criticalDamage = _equipment.GetCriticalDamage();
            _attackSpeed = _equipment.GetAttackSpeed();
        }
        else
        {
            // 장비 없을 때 기본값
            _maxHp = 100f;
            _attack = 10f;
            _defense = 0f;
            _criticalChance = 0.05f;
            _criticalDamage = 100f; // 100% 추가 = 2배 데미지
            _attackSpeed = 1f;
        }

        // 투자 스탯 초기화
        _damageIncrease = 0f;
        _cooldownReduction = 0f;
        _manaReduction = 0f;

        // StatTree 투자 효과 적용
        if (_allocation != null)
        {
            var modifiers = _allocation.GetTotalModifiers();
            ApplyModifiers(modifiers);
        }

        // 스탯 변경 이벤트 발생
        OnStatsChanged?.Invoke();
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
                    _attackSpeed *= (1f + mod.Value / 100f);
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
