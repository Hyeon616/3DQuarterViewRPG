using Mirror;
using UnityEngine;

public class PlayerCombatController : NetworkBehaviour
{
    [Header("Basic Attack")]
    [SerializeField] private AttackType basicAttackType;

    [Header("Hit Detection")]
    [SerializeField] private float hitRadius = 0.67f;
    [SerializeField] private LayerMask hitLayerMask;

    private CombatManager _combatManager;
    private PlayerEvents _events;

    private void Awake()
    {
        var moveController = GetComponent<PlayerMoveController>();
        if (moveController != null)
        {
            _events = moveController.Events;
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        int playerLayer = LayerMask.NameToLayer("Player");
        _combatManager = new CombatManager(playerLayer);
    }

    private void OnEnable()
    {
        if (_events != null)
        {
            _events.OnAttackStarted += HandleAttackStarted;
        }
    }

    private void OnDisable()
    {
        if (_events != null)
        {
            _events.OnAttackStarted -= HandleAttackStarted;
        }
    }

    [Server]
    private void HandleAttackStarted(int comboIndex)
    {
        Debug.Log($"[Combat] Attack started - Combo: {comboIndex}, Type: {basicAttackType}");
        DetectHits();
    }

    [Server]
    private void DetectHits()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius, hitLayerMask);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            HitDirection direction = _combatManager.GetHitDirection(transform.position, hit.transform);
            bool isBonusHit = _combatManager.CheckBonusHit(basicAttackType, gameObject, hit.transform);
            HitBonus bonus = _combatManager.GetHitBonus(basicAttackType, gameObject, hit.transform);

            RpcLogHit(hit.gameObject.name, (int)direction, isBonusHit, bonus.DamageMultiplier);
        }
    }

    [ClientRpc]
    private void RpcLogHit(string targetName, int direction, bool isBonusHit, float damageMultiplier)
    {
        Debug.Log($"[Hit] Target: {targetName}, Direction: {(HitDirection)direction}, BonusHit: {isBonusHit}, DamageMultiplier: {damageMultiplier:F2}");
    }

    public AttackType BasicAttackType => basicAttackType;

    [Server]
    public bool CheckBonusHit(Transform target)
    {
        if (_combatManager == null)
            return false;

        return _combatManager.CheckBonusHit(basicAttackType, gameObject, target);
    }

    [Server]
    public HitBonus GetHitBonus(Transform target)
    {
        if (_combatManager == null)
            return new HitBonus(1f, 0f, 0f);

        return _combatManager.GetHitBonus(basicAttackType, gameObject, target);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}