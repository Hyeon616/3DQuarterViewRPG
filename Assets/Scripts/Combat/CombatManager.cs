using UnityEngine;

public class CombatManager
{
    private readonly int _playerLayer;
    private readonly float _headAngle;
    private readonly float _backAngle;
    private readonly HitBonus _noneBonus;
    private readonly HitBonus _backBonus;
    private readonly HitBonus _headBonus;

    public CombatManager(
        int playerLayer,
        float headAngle = 30f,
        float backAngle = 30f,
        float backDamageMultiplier = 1.05f,
        float backCriticalChanceBonus = 0.1f,
        float headDamageMultiplier = 1.2f,
        float headStaggerDamageBonus = 0.1f)
    {
        _playerLayer = playerLayer;
        _headAngle = headAngle;
        _backAngle = backAngle;
        _noneBonus = new HitBonus(1f, 0f, 0f);
        _backBonus = new HitBonus(backDamageMultiplier, backCriticalChanceBonus, 0f);
        _headBonus = new HitBonus(headDamageMultiplier, 0f, headStaggerDamageBonus);
    }

    public HitDirection GetHitDirection(Vector3 attackerPosition, Transform target)
    {
        Vector3 toAttacker = attackerPosition - target.position;
        toAttacker.y = 0f;
        toAttacker.Normalize();

        Vector3 targetForward = target.forward;
        targetForward.y = 0f;
        targetForward.Normalize();

        float angle = Vector3.Angle(targetForward, toAttacker);

        if (angle <= _headAngle)
        {
            return HitDirection.Head;
        }

        if (angle >= 180f - _backAngle)
        {
            return HitDirection.Back;
        }

        return HitDirection.Normal;
    }

    public HitDirection GetHitDirection(Transform attacker, Transform target)
    {
        return GetHitDirection(attacker.position, target);
    }

    public bool IsBonusHit(AttackType attackType, HitDirection hitDirection)
    {
        return attackType switch
        {
            AttackType.Head => hitDirection == HitDirection.Head,
            AttackType.Back => hitDirection == HitDirection.Back,
            _ => false
        };
    }

    public bool IsPlayerToPlayer(GameObject attacker, GameObject target)
    {
        return attacker.layer == _playerLayer && target.layer == _playerLayer;
    }

    public bool CheckBonusHit(AttackType attackType, GameObject attacker, Transform target)
    {
        if (attackType == AttackType.Normal)
            return false;

        if (IsPlayerToPlayer(attacker, target.gameObject))
            return false;

        HitDirection hitDirection = GetHitDirection(attacker.transform.position, target);
        return IsBonusHit(attackType, hitDirection);
    }

    public HitBonus GetHitBonus(AttackType attackType, GameObject attacker, Transform target)
    {
        if (attackType == AttackType.Normal)
            return _noneBonus;

        if (IsPlayerToPlayer(attacker, target.gameObject))
            return _noneBonus;

        HitDirection hitDirection = GetHitDirection(attacker.transform.position, target);

        if (!IsBonusHit(attackType, hitDirection))
            return _noneBonus;

        return hitDirection switch
        {
            HitDirection.Back => _backBonus,
            HitDirection.Head => _headBonus,
            _ => _noneBonus
        };
    }
}