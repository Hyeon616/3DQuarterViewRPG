using Mirror;
using UnityEngine;

public class PlayerCombatController : NetworkBehaviour
{
    [Header("Hit Detection")]
    [SerializeField] private float searchRadius = 5f;
    [SerializeField] private LayerMask hitLayerMask;

    private NetworkEffectPool _effectPool;
    private CombatManager _combatManager;
    private PlayerEvents _events;
    private SkillData _currentSkill;

    public override void OnStartServer()
    {
        base.OnStartServer();
        int playerLayer = LayerMask.NameToLayer("Player");
        _combatManager = new CombatManager(playerLayer);
        _effectPool = FindAnyObjectByType<NetworkEffectPool>();

        var moveController = GetComponent<PlayerMoveController>();
        if (moveController != null)
        {
            _events = moveController.Events;
            _events.OnSkillStarted += SkillExcute;
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (_events != null)
        {
            _events.OnSkillStarted -= SkillExcute;
        }
    }

    [Server]
    private void SkillExcute(SkillData skill)
    {
        _currentSkill = skill;
        DetectHits();
        SpawnSkillEffect(skill);
    }

    [Server]
    private void SpawnSkillEffect(SkillData skill)
    {
        if (skill.EffectPrefab == null || _effectPool == null) return;

        Vector3 worldOffset = transform.TransformDirection(skill.EffectOffset);
        Vector3 effectPosition = transform.position + worldOffset;

        _effectPool.SpawnEffectOnClients(skill.EffectPrefab.name, effectPosition, transform.rotation);
    }

    [Server]
    private void DetectHits()
    {
        if (_currentSkill == null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, hitLayerMask);
        float halfAngle = _currentSkill.HitAngle * 0.5f;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            dirToTarget.y = 0f;

            // 각도 체크 (360도면 전방위)
            if (_currentSkill.HitAngle < 360f)
            {
                float angle = Vector3.Angle(transform.forward, dirToTarget);
                if (angle > halfAngle)
                    continue;
            }

            float hitRange = GetSkillHitRange(_currentSkill, hit);
            float distanceToTarget = Vector3.Distance(transform.position, hit.transform.position);
            if (distanceToTarget > hitRange)
                continue;

            HitDirection hitDirection = _combatManager.GetHitDirection(transform.position, hit.transform);
            HitBonusData bonus = _combatManager.GetHitBonus(_currentSkill.AttackType, gameObject, hit.transform);

            var damageable = hit.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(_currentSkill.BaseDamage, bonus, gameObject, DamageType.Normal, _currentSkill.AttackType, hitDirection);
            }
        }
    }

    public AttackType GetCurrentAttackType()
    {
        return _currentSkill?.AttackType ?? AttackType.Normal;
    }

    [Server]
    public bool CheckBonusHit(Transform target)
    {
        if (_combatManager == null)
            return false;

        var attackType = _currentSkill?.AttackType ?? AttackType.Normal;
        return _combatManager.CheckBonusHit(attackType, gameObject, target);
    }

    [Server]
    public HitBonusData GetHitBonus(Transform target)
    {
        if (_combatManager == null)
            return new HitBonusData(1f, 0f, 0f);

        var attackType = _currentSkill?.AttackType ?? AttackType.Normal;
        return _combatManager.GetHitBonus(attackType, gameObject, target);
    }

    private float GetSkillHitRange(SkillData skill, Collider target)
    {
        float baseRange = skill.HitRange;
        float colliderBonus = target switch
        {
            SphereCollider sphere => sphere.radius,
            CapsuleCollider capsule => capsule.radius,
            BoxCollider box => Mathf.Max(box.size.x, box.size.z) * 0.5f,
            _ => 0f
        };
        return baseRange + colliderBonus;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}
