using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerAttackController : NetworkBehaviour
{
    [Header("Hold Attack")]
    [SerializeField] private float holdAttackInterval = 0.15f;
    [SerializeField] private LayerMask groundLayerMask;

    private PlayerAnimationController animController;
    private Camera mainCamera;
    private float lastAttackRequestTime;
    private bool attackStarted;

    private void Awake()
    {
        animController = GetComponent<PlayerAnimationController>();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        mainCamera = Camera.main;
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
                RequestAttack();
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
            RequestAttack();
        }
    }

    private void RequestAttack()
    {
        Vector3 attackDirection = GetAttackDirection();
        animController.CmdAttack(attackDirection);
    }

    private Vector3 GetAttackDirection()
    {
        if (mainCamera == null || Mouse.current == null)
            return transform.forward;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayerMask))
        {
            Vector3 direction = hit.point - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
                return direction.normalized;
        }

        return transform.forward;
    }
}
