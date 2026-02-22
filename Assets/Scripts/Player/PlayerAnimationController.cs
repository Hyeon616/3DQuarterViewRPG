using Mirror;
using UnityEngine;

[RequireComponent(typeof(PlayerMoveController))]
public class PlayerAnimationController : NetworkBehaviour
{
    [SerializeField] private float moveThreshold = 0.3f;

    [SyncVar(hook = nameof(OnAnimationChanged))]
    private string currentAnimation = BaseAnimationData.Idle;

    [SyncVar] private bool isAttacking;

    private PlayerMoveController moveController;
    private IAnimatable animatable;

    private int currentComboIndex;
    private float attackStartTime;
    private float currentAttackDuration;
    private bool nextAttackQueued;

    // 공격 이동
    private Vector3 attackMoveDirection;
    private float attackMoveProgress;
    private float attackMoveSpeed;
    private float currentAttackMoveDuration;

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
        UpdateAttackState();
        UpdateAnimationState();
    }

    [Server]
    private void UpdateAttackMovement()
    {
        if (!isAttacking || attackMoveProgress >= 1f || currentAttackMoveDuration <= 0f) return;

        float moveDelta = attackMoveSpeed * Time.deltaTime;
        attackMoveProgress += Time.deltaTime / currentAttackMoveDuration;
        attackMoveProgress = Mathf.Clamp01(attackMoveProgress);

        var agent = moveController.Agent;
        if (agent != null && agent.enabled)
        {
            agent.Move(attackMoveDirection * moveDelta);
        }
    }

    [Server]
    private void UpdateAttackState()
    {
        if (!isAttacking) return;

        float elapsed = Time.time - attackStartTime;

        if (elapsed >= currentAttackDuration)
        {
            if (nextAttackQueued)
            {
                nextAttackQueued = false;
                ExecuteNextAttack();
            }
            else
            {
                isAttacking = false;
            }
        }
    }

    [Server]
    private void UpdateAnimationState()
    {
        if (isAttacking) return;

        string targetAnim = DetermineAnimation();

        if (currentAnimation != targetAnim)
        {
            currentAnimation = targetAnim;
        }
    }

    [Server]
    private string DetermineAnimation()
    {
        if (moveController.Agent != null &&
            moveController.Agent.velocity.sqrMagnitude > moveThreshold * moveThreshold)
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
    public void CmdAttack()
    {
        if (MaxComboCount == 0) return;

        if (isAttacking)
        {
            nextAttackQueued = true;
            return;
        }

        currentComboIndex = 0;
        ExecuteNextAttack();
    }

    [Server]
    private void ExecuteNextAttack()
    {
        string attackAnim = BaseAnimationData.GetAttackName(currentComboIndex);

        currentAnimation = attackAnim;
        isAttacking = true;
        attackStartTime = Time.time;
        currentAttackDuration = animatable?.GetAnimationDuration(attackAnim) ?? 1f;

        // 공격 이동 데이터 가져오기
        var moveData = animatable?.GetAttackMoveData(attackAnim) ?? (0f, 0f);
        attackMoveDirection = transform.forward;
        attackMoveProgress = 0f;
        currentAttackMoveDuration = moveData.duration;
        attackMoveSpeed = moveData.duration > 0f ? moveData.distance / moveData.duration : 0f;

        currentComboIndex = (currentComboIndex + 1) % MaxComboCount;
    }
}
