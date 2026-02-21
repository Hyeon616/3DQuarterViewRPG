using Mirror;
using UnityEngine;

[RequireComponent(typeof(PlayerMoveController))]
public class PlayerAnimationController : NetworkBehaviour
{
    [SerializeField] private float moveThreshold = 0.3f;

    [SyncVar(hook = nameof(OnAnimationChanged))]
    private string currentAnimation = BaseAnimationData.Idle;

    private PlayerMoveController moveController;
    private IAnimatable animatable;

    private void Awake()
    {
        moveController = GetComponent<PlayerMoveController>();
        animatable = GetComponent<IAnimatable>();
    }

    private void Update()
    {
        if (!isServer) return;
        UpdateAnimationState();
    }

    [Server]
    private void UpdateAnimationState()
    {
        string targetAnim = DetermineAnimation();

        if (currentAnimation != targetAnim)
        {
            currentAnimation = targetAnim;
        }
    }

    [Server]
    private string DetermineAnimation()
    {
        // 이동 상태 체크
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

    // 서버에서 직접 애니메이션을 설정할 때 사용 (공격, 피격 등)
    [Server]
    public void SetAnimation(string type)
    {
        currentAnimation = type;
    }

    // 클라이언트에서 서버에 애니메이션 요청
    [Command]
    public void CmdSetAnimation(string type)
    {
        currentAnimation = type;
    }
}