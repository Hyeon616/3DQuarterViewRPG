using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackController : NetworkBehaviour
{
    [Header("Hold Attack")]
    [SerializeField] private float holdAttackInterval = 0.15f;
    [SerializeField] private LayerMask groundLayerMask;

    private PlayerEvents _events;
    private Camera _mainCamera;
    private float _lastAttackRequestTime;
    private bool _attackStarted;

    private void Awake()
    {
        var moveController = GetComponent<PlayerMoveController>();
        if (moveController != null)
        {
            _events = moveController.Events;
        }
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!isOwned) return;

        bool isHold = Mouse.current != null && Mouse.current.leftButton.isPressed;

        if (isHold && _attackStarted)
        {
            if (Time.time - _lastAttackRequestTime >= holdAttackInterval)
            {
                _lastAttackRequestTime = Time.time;
                RequestAttack();
            }
        }

        if (!isHold)
        {
            _attackStarted = false;
        }
    }

    public void OnAttack(InputValue value)
    {
        if (!isOwned) return;

        if (value.isPressed)
        {
            _attackStarted = true;
            _lastAttackRequestTime = Time.time;
            RequestAttack();
        }
    }

    private void RequestAttack()
    {
        Vector3 attackDirection = GetAttackDirection();
        _events?.RequestAttack(attackDirection);
    }

    private Vector3 GetAttackDirection()
    {
        if (_mainCamera == null || Mouse.current == null)
            return transform.forward;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mousePos);

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