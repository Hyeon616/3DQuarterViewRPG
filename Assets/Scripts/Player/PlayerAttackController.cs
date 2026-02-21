using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerAttackController : NetworkBehaviour
{
    [Header("Hold Attack")]
    [SerializeField] private float holdAttackInterval = 0.15f; // 홀드 시 공격 요청 간격

    private PlayerAnimationController animController;
    private float lastAttackRequestTime;
    private bool attackStarted;

    private void Awake()
    {
        animController = GetComponent<PlayerAnimationController>();
    }

    private void Update()
    {
        if (!isOwned) return;

        bool isHeld = Mouse.current != null && Mouse.current.leftButton.isPressed;

        // 홀드 중일 때 일정 간격으로 공격 요청
        if (isHeld && attackStarted)
        {
            if (Time.time - lastAttackRequestTime >= holdAttackInterval)
            {
                lastAttackRequestTime = Time.time;
                animController.CmdAttack();
            }
        }

        // 버튼을 떼면 attackStarted 리셋
        if (!isHeld)
        {
            attackStarted = false;
        }
    }

    // PlayerInput에서 Attack 액션 호출 (좌클릭)
    public void OnAttack(InputValue value)
    {
        if (!isOwned) return;

        // 버튼을 누르는 순간 즉시 공격
        if (value.isPressed)
        {
            attackStarted = true;
            lastAttackRequestTime = Time.time;
            animController.CmdAttack();
        }
    }
}
