using Mirror;
using UnityEngine;

[RequireComponent(typeof(PlayerMoveController))]
public class PlayerAnimationController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveThreshold = 0.3f;

    [Header("Attack")]
    [SerializeField] private float defaultAttackDuration = 1f;

    [SyncVar(hook = nameof(OnAnimationChanged))]
    private string currentAnimation = BaseAnimationData.Idle;

    [SyncVar] private bool isAttacking;

    private PlayerMoveController moveController;
    private IAnimatable animatable;

    private int currentComboIndex;
    private float attackStartTime;
    private float currentAttackDuration;
    private bool nextAttackQueued;

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
        UpdateAttackState();
        UpdateAnimationState();
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
        float duration = animatable?.GetAnimationDuration(attackAnim) ?? 0f;

        if (duration <= 0f)
            duration = defaultAttackDuration;

        currentAnimation = attackAnim;
        isAttacking = true;
        attackStartTime = Time.time;
        currentAttackDuration = duration;

        currentComboIndex = (currentComboIndex + 1) % MaxComboCount;
    }
}
