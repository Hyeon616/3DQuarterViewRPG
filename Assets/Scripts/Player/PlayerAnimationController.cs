using Mirror;
using UnityEngine;

public class PlayerAnimationController : NetworkBehaviour, IAttackState
{
    private struct SkillMoveState
    {
        public Vector3 Direction;
        public float TotalDistance;
        public float MovedDistance;
        public float Duration;
        public float ElapsedTime;

        public bool IsActive => ElapsedTime < Duration && Duration > 0f;
        public float Progress => Duration > 0f ? Mathf.Clamp01(ElapsedTime / Duration) : 1f;

        public void Start(Vector3 direction, float distance, float duration)
        {
            Direction = direction;
            TotalDistance = distance;
            MovedDistance = 0f;
            Duration = duration;
            ElapsedTime = 0f;
        }

        public float GetMoveDelta(float deltaTime)
        {
            if (!IsActive || TotalDistance <= 0f) return 0f;

            ElapsedTime += deltaTime;
            float targetDistance = (ElapsedTime / Duration) * TotalDistance;
            float delta = targetDistance - MovedDistance;
            MovedDistance = targetDistance;
            return delta;
        }
    }

    [SyncVar(hook = nameof(OnAnimationChanged))]
    private string _currentAnimation = BaseAnimationData.Idle;

    [SyncVar] private bool _isUsingSkill;
    [SyncVar] private int _currentComboIndex;

    private Rigidbody _rigidbody;
    private IAnimatable _animatable;
    private PlayerEvents _events;
    private SkillData _currentSkill;

    private float _skillStartTime;
    private float _currentSkillDuration;
    private bool _nextSkillQueued;
    private Vector3 _queuedSkillDirection;

    private SkillMoveState _skillMove;

    private int MaxComboCount => _animatable?.AttackCount ?? 0;
    public bool IsAttacking => _isUsingSkill;
    public int CurrentComboIndex => _currentComboIndex;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animatable = GetComponent<IAnimatable>();

        var moveController = GetComponent<PlayerMoveController>();
        if (moveController != null)
        {
            _events = moveController.Events;
        }
    }

    private void OnEnable()
    {
        if (_events != null)
        {
            _events.OnSkillRequested += HandleSkillRequested;
        }
    }

    private void OnDisable()
    {
        if (_events != null)
        {
            _events.OnSkillRequested -= HandleSkillRequested;
        }
    }

    private void HandleSkillRequested(Vector3 direction)
    {
        CmdUseSkill(direction);
    }

    private void Update()
    {
        if (!isServer) return;
        ProcessSkill();
        UpdateLocomotionAnimation();
    }

    private void FixedUpdate()
    {
        if (!isServer) return;
        UpdateSkillMovement();
    }

    [Server]
    private void UpdateSkillMovement()
    {
        if (!_isUsingSkill || !_skillMove.IsActive) return;

        float moveDelta = _skillMove.GetMoveDelta(Time.fixedDeltaTime);
        if (moveDelta > 0f)
        {
            Vector3 movement = _skillMove.Direction * moveDelta;
            _rigidbody.MovePosition(_rigidbody.position + movement);
        }
    }

    [Server]
    private void ProcessSkill()
    {
        if (!_isUsingSkill) return;

        float elapsed = Time.time - _skillStartTime;

        if (elapsed >= _currentSkillDuration)
        {
            if (_nextSkillQueued)
            {
                _nextSkillQueued = false;
                ExecuteNextSkill(_queuedSkillDirection);
            }
            else
            {
                _isUsingSkill = false;
                _currentSkill = null;
                _events?.EndSkill();
            }
        }
    }

    [Server]
    private void UpdateLocomotionAnimation()
    {
        if (_isUsingSkill) return;

        string targetAnim = GetLocomotionAnimation();

        if (_currentAnimation != targetAnim)
        {
            _currentAnimation = targetAnim;
        }
    }

    [Server]
    private string GetLocomotionAnimation()
    {
        float threshold = _animatable?.RunThreshold ?? 0.3f;
        Vector3 velocity = _rigidbody != null ? _rigidbody.velocity : Vector3.zero;

        if (velocity.sqrMagnitude > threshold * threshold)
        {
            return BaseAnimationData.Run;
        }

        return BaseAnimationData.Idle;
    }

    private void OnAnimationChanged(string oldAnim, string newAnim)
    {
        _animatable?.PlayAnimation(newAnim);
    }

    [Command]
    public void CmdUseSkill(Vector3 direction)
    {
        if (MaxComboCount == 0) return;

        if (_isUsingSkill)
        {
            _nextSkillQueued = true;
            _queuedSkillDirection = direction;
            return;
        }

        _currentComboIndex = 0;
        ExecuteNextSkill(direction);
    }

    [Server]
    private void ExecuteNextSkill(Vector3 direction)
    {
        var skill = _animatable?.GetBasicAttack(_currentComboIndex);
        if (skill == null) return;

        _currentSkill = skill;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        string skillAnim = BaseAnimationData.GetAttackName(_currentComboIndex);

        _currentAnimation = skillAnim;
        _isUsingSkill = true;
        _skillStartTime = Time.time;
        _currentSkillDuration = skill.Clip != null ? skill.Clip.length : 1f;

        _skillMove.Start(transform.forward, skill.MoveDistance, skill.MoveDuration);

        _events?.StartSkill(skill);

        _currentComboIndex = (_currentComboIndex + 1) % MaxComboCount;
    }
}
