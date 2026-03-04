using Mirror;
using UnityEngine;

public class PlayerCombatController : NetworkBehaviour
{
    [Header("Hit Detection")]
    [SerializeField] private float searchRadius = 5f;
    [SerializeField] private CharacterData characterData;

    private NetworkEffectPool _effectPool;
    private NetworkSoundPool _soundPool;
    private EffectData _effectDatabase;
    private CombatManager _combatManager;
    private PlayerEvents _events;
    private SkillData _currentSkill;

    public override void OnStartServer()
    {
        base.OnStartServer();
        int playerLayer = LayerMask.NameToLayer("Player");
        _combatManager = new CombatManager(playerLayer);
        _effectPool = FindAnyObjectByType<NetworkEffectPool>();
        _soundPool = FindAnyObjectByType<NetworkSoundPool>();
        _effectDatabase = _effectPool?.EffectData;

        var moveController = GetComponent<PlayerMoveController>();
        if (moveController != null)
        {
            _events = moveController.Events;
            _events.OnSkillStarted += SkillExecute;
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (_events != null)
        {
            _events.OnSkillStarted -= SkillExecute;
        }
    }

    [Server]
    private void SkillExecute(SkillData skill)
    {
        _currentSkill = skill;
        PlayCastSound(skill);
        DetectHits();
        SpawnSkillEffect(skill);
    }

    [Server]
    private void PlayCastSound(SkillData skill)
    {
        if (skill.CastSound == null || _soundPool == null) return;
        _soundPool.PlaySoundOnClients(skill.CastSound.name, transform.position, netIdentity);
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

        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, characterData.HitLayerMask);
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
                SpawnHitEffect(hit, hitDirection);
            }
        }
    }

    [Server]
    private void SpawnHitEffect(Collider targetCollider, HitDirection hitDirection)
    {
        Vector3 effectPosition = GetHitEffectPosition(targetCollider);

        SpawnHitVisualEffect(hitDirection, effectPosition);
        PlayHitSound(effectPosition);
    }

    [Server]
    private void SpawnHitVisualEffect(HitDirection hitDirection, Vector3 effectPosition)
    {
        if (_effectPool == null || _effectDatabase == null) return;

        bool isBonus = _currentSkill.AttackType switch
        {
            AttackType.Head => hitDirection == HitDirection.Head,
            AttackType.Back => hitDirection == HitDirection.Back,
            _ => false
        };

        GameObject effectPrefab = isBonus && _effectDatabase.DefaultBonusHitEffectPrefab != null
            ? _effectDatabase.DefaultBonusHitEffectPrefab
            : _effectDatabase.DefaultHitEffectPrefab;

        if (effectPrefab == null) return;

        Quaternion effectRotation = Quaternion.LookRotation(transform.forward);
        _effectPool.SpawnEffectOnClients(effectPrefab.name, effectPosition, effectRotation);
    }

    [Server]
    private void PlayHitSound(Vector3 position)
    {
        if (_currentSkill?.HitSound == null || _soundPool == null) return;
        _soundPool.PlaySoundOnClients(_currentSkill.HitSound.name, position, netIdentity);
    }

    private Vector3 GetHitEffectPosition(Collider targetCollider)
    {
        Vector3 targetCenter = targetCollider.bounds.center;
        Vector3 directionToAttacker = (transform.position - targetCenter).normalized;

        Vector3 surfacePoint = targetCollider.ClosestPoint(transform.position);
        surfacePoint.y = targetCenter.y;

        return surfacePoint;
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
