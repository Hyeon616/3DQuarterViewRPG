using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerAttackController : NetworkBehaviour
{
    [Header("Hold Attack")]
    [SerializeField] private float holdAttackInterval = 0.15f; 

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

        bool isHold = Mouse.current != null && Mouse.current.leftButton.isPressed;

        if (isHold && attackStarted)
        {
            if (Time.time - lastAttackRequestTime >= holdAttackInterval)
            {
                lastAttackRequestTime = Time.time;
                animController.CmdAttack();
            }
        }

        if (!isHold)
        {
            attackStarted = false;
        }
    }

    public void OnAttack(InputValue value)
    {
        if (!isOwned) return;

        if (value.isPressed)
        {
            attackStarted = true;
            lastAttackRequestTime = Time.time;
            animController.CmdAttack();
        }
    }
}
