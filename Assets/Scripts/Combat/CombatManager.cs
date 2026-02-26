using UnityEngine;

public class CombatManager
{
    private readonly int _playerLayer;
    private readonly HitDirectionDetector _directionDetector;
    private readonly HitBonus _hitBonus;

    public CombatManager(int playerLayer)
    {
        _playerLayer = playerLayer;
        _directionDetector = new HitDirectionDetector();
        _hitBonus = new HitBonus();
    }

    public CombatManager(int playerLayer, HitBonus hitBonus)
    {
        _playerLayer = playerLayer;
        _directionDetector = new HitDirectionDetector();
        _hitBonus = hitBonus;
    }

    public HitDirection GetHitDirection(Vector3 attackerPosition, Transform target)
    {
        return _directionDetector.Detect(attackerPosition, target);
    }

    public HitDirection GetHitDirection(Transform attacker, Transform target)
    {
        return _directionDetector.Detect(attacker, target);
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
        return _hitBonus.IsBonusHit(attackType, hitDirection);
    }

    public HitBonusData GetHitBonus(AttackType attackType, GameObject attacker, Transform target)
    {
        if (attackType == AttackType.Normal)
            return _hitBonus.NoneBonus;

        if (IsPlayerToPlayer(attacker, target.gameObject))
            return _hitBonus.NoneBonus;

        HitDirection hitDirection = GetHitDirection(attacker.transform.position, target);

        if (!_hitBonus.IsBonusHit(attackType, hitDirection))
            return _hitBonus.NoneBonus;

        return _hitBonus.GetBonus(hitDirection);
    }
}