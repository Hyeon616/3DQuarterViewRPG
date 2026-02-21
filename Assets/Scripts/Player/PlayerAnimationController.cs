using Mirror;
using UnityEngine;

[RequireComponent(typeof(PlayerMoveController))]
public class PlayerAnimationController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveThreshold = 0.3f;

    [Header("Combo Attack")]
    [SerializeField] private float comboResetTime = 1.0f;
    [SerializeField] private float comboCancelWindowRatio = 0.3f; // 공격 애니메이션의 마지막 30%에서 다음 공격 입력 가능

    [SyncVar(hook = nameof(OnAnimationChanged))]
    private string currentAnimation = BaseAnimationData.Idle;

    private PlayerMoveController moveController;
    private IAnimatable animatable;

    // 콤보 상태 (서버)
    private int currentComboIndex;
    private float attackStartTime;
    private float currentAttackDuration;
    [SyncVar] private bool isAttacking;
    private bool nextAttackQueued; // 다음 공격 예약 여부

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

        // 공격 종료
        if (elapsed >= currentAttackDuration)
        {
            // 다음 공격이 예약되어 있으면 실행
            if (nextAttackQueued)
            {
                nextAttackQueued = false;
                ExecuteNextAttack();
            }
            else
            {
                // 공격 종료, Idle로 복귀
                isAttacking = false;
            }
        }
    }

    [Server]
    private void UpdateAnimationState()
    {
        // 공격 중일 때는 이동 애니메이션으로 전환하지 않음
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

        // 이미 공격 중인 경우 - 다음 공격 예약
        if (isAttacking)
        {
            nextAttackQueued = true;
            return;
        }

        // 공격 중이 아닐 때는 항상 Attack1부터 시작
        currentComboIndex = 0;
        ExecuteNextAttack();
    }

    [Server]
    private void ExecuteNextAttack()
    {
        string attackAnim = BaseAnimationData.GetAttackName(currentComboIndex);
        float duration = animatable?.GetAnimationDuration(attackAnim) ?? 0f;

        // duration이 0이면 기본값 사용 (서버에 애니메이션 데이터가 없을 수 있음)
        if (duration <= 0f)
            duration = 1f;

        currentAnimation = attackAnim;

        // 공격 상태 설정
        isAttacking = true;
        attackStartTime = Time.time;
        currentAttackDuration = duration;

        // 다음 콤보 준비
        currentComboIndex = (currentComboIndex + 1) % MaxComboCount;
    }

    [Server]
    public void SetAnimation(string type)
    {
        currentAnimation = type;
    }

    [Command]
    public void CmdSetAnimation(string type)
    {
        currentAnimation = type;
    }
}
