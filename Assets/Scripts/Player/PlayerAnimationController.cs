using Mirror;
using UnityEngine;

public class PlayerAnimationController : NetworkBehaviour, IAttackState
{
    private struct AttackMoveState
    {
        public Vector3 Direction;
        public float Progress;
        public float Speed;
        public float Duration;

        public void Reset(Vector3 direction, float distance, float duration)
        {
            Direction = direction;
            Progress = 0f;
            Duration = duration;
            Speed = duration > 0f ? distance / duration : 0f;
        }
    }

    [SyncVar(hook = nameof(OnAnimationChanged))]
    private string _currentAnimation = BaseAnimationData.Idle;

    [SyncVar] private bool _isAttacking;
    [SyncVar] private int _currentComboIndex;

    private IMovement _movementProvider;
    private IAnimatable _animatable;
    private PlayerEvents _events;

    private float _attackStartTime;
    private float _currentAttackDuration;
    private bool _nextAttackQueued;

    private AttackMoveState _attackMove;
    private Vector3 _queuedAttackDirection;

    private int MaxComboCount => _animatable?.AttackCount ?? 0;
    public bool IsAttacking => _isAttacking;
    public int CurrentComboIndex => _currentComboIndex;

    private void Awake()
    {
        _movementProvider = GetComponent<IMovement>();
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
            _events.OnAttackRequested += HandleAttackRequested;
        }
    }

    private void OnDisable()
    {
        if (_events != null)
        {
            _events.OnAttackRequested -= HandleAttackRequested;
        }
    }

    private void HandleAttackRequested(Vector3 direction)
    {
        CmdAttack(direction);
    }

    private void Update()
    {
        if (!isServer) return;
        ProcessAttack();
        UpdateLocomotionAnimation();
    }

    private void FixedUpdate()
    {
        if (!isServer) return;
        UpdateAttackMovement();
    }

    [Server]
    private void UpdateAttackMovement()
    {
        if (!_isAttacking || _attackMove.Progress >= 1f || _attackMove.Duration <= 0f) return;

        float moveDelta = _attackMove.Speed * Time.fixedDeltaTime;
        _attackMove.Progress += Time.fixedDeltaTime / _attackMove.Duration;
        _attackMove.Progress = Mathf.Clamp01(_attackMove.Progress);

        _movementProvider?.Move(_attackMove.Direction * moveDelta);
    }

    [Server]
    private void ProcessAttack()
    {
        if (!_isAttacking) return;

        float elapsed = Time.time - _attackStartTime;

        if (elapsed >= _currentAttackDuration)
        {
            if (_nextAttackQueued)
            {
                _nextAttackQueued = false;
                ExecuteNextAttack(_queuedAttackDirection);
            }
            else
            {
                _isAttacking = false;
                _events?.EndAttack();
            }
        }
    }

    [Server]
    private void UpdateLocomotionAnimation()
    {
        if (_isAttacking) return;

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
        Vector3 velocity = _movementProvider?.Velocity ?? Vector3.zero;

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
    public void CmdAttack(Vector3 direction)
    {
        if (MaxComboCount == 0) return;

        if (_isAttacking)
        {
            _nextAttackQueued = true;
            _queuedAttackDirection = direction;
            return;
        }

        _currentComboIndex = 0;
        ExecuteNextAttack(direction);
    }

    [Server]
    private void ExecuteNextAttack(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        string attackAnim = BaseAnimationData.GetAttackName(_currentComboIndex);

        _currentAnimation = attackAnim;
        _isAttacking = true;
        _attackStartTime = Time.time;
        _currentAttackDuration = _animatable?.GetAnimationDuration(attackAnim) ?? 1f;

        var moveData = _animatable?.GetAttackMoveData(attackAnim) ?? (0f, 0f);
        _attackMove.Reset(transform.forward, moveData.distance, moveData.duration);

        _events?.StartAttack(_currentComboIndex);

        _currentComboIndex = (_currentComboIndex + 1) % MaxComboCount;
    }
}