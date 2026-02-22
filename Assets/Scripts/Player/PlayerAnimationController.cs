using Mirror;
using UnityEngine;

[RequireComponent(typeof(PlayerMoveController))]
public class PlayerAnimationController : NetworkBehaviour
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
    private string currentAnimation = BaseAnimationData.Idle;

    [SyncVar] private bool isAttacking;

    private PlayerMoveController moveController;
    private IAnimatable animatable;

    private int currentComboIndex;
    private float attackStartTime;
    private float currentAttackDuration;
    private bool nextAttackQueued;

    private AttackMoveState attackMove;
    private Vector3 queuedAttackDirection;

    private int MaxComboCount => animatable?.AttackCount ?? 0;
    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        moveController = GetComponent<PlayerMoveController>();
        animatable = GetComponent<IAnimatable>();
    }

    private void Update()
    {
        if (!isServer) return;
        UpdateAttackMovement();
        ProcessAttack();
        UpdateLocomotionAnimation();
    }

    [Server]
    private void UpdateAttackMovement()
    {
        if (!isAttacking || attackMove.Progress >= 1f || attackMove.Duration <= 0f) return;

        float moveDelta = attackMove.Speed * Time.deltaTime;
        attackMove.Progress += Time.deltaTime / attackMove.Duration;
        attackMove.Progress = Mathf.Clamp01(attackMove.Progress);

        var agent = moveController.Agent;
        if (agent != null && agent.enabled)
        {
            agent.Move(attackMove.Direction * moveDelta);
        }
    }

    [Server]
    private void ProcessAttack()
    {
        if (!isAttacking) return;

        float elapsed = Time.time - attackStartTime;

        if (elapsed >= currentAttackDuration)
        {
            if (nextAttackQueued)
            {
                nextAttackQueued = false;
                ExecuteNextAttack(queuedAttackDirection);
            }
            else
            {
                isAttacking = false;
            }
        }
    }

    [Server]
    private void UpdateLocomotionAnimation()
    {
        if (isAttacking) return;

        string targetAnim = GetLocomotionAnimation();

        if (currentAnimation != targetAnim)
        {
            currentAnimation = targetAnim;
        }
    }

    [Server]
    private string GetLocomotionAnimation()
    {
        float threshold = animatable?.RunThreshold ?? 0.3f;

        if (moveController.Agent != null &&
            moveController.Agent.velocity.sqrMagnitude > threshold * threshold)
        {
            return BaseAnimationData.Run;
        }

        return BaseAnimationData.Idle;
    }

    private void OnAnimationChanged(string oldAnim, string newAnim)
    {
        animatable?.PlayAnimation(newAnim);
    }

    [Command]
    public void CmdAttack(Vector3 direction)
    {
        if (MaxComboCount == 0) return;

        if (isAttacking)
        {
            nextAttackQueued = true;
            queuedAttackDirection = direction;
            return;
        }

        currentComboIndex = 0;
        ExecuteNextAttack(direction);
    }

    [Server]
    private void ExecuteNextAttack(Vector3 direction)
    {
        // 공격 방향으로 회전
        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        string attackAnim = BaseAnimationData.GetAttackName(currentComboIndex);

        currentAnimation = attackAnim;
        isAttacking = true;
        attackStartTime = Time.time;
        currentAttackDuration = animatable?.GetAnimationDuration(attackAnim) ?? 1f;

        var moveData = animatable?.GetAttackMoveData(attackAnim) ?? (0f, 0f);
        attackMove.Reset(transform.forward, moveData.distance, moveData.duration);

        currentComboIndex = (currentComboIndex + 1) % MaxComboCount;
    }
}
