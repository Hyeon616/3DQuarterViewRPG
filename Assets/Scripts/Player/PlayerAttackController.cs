using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackController : NetworkBehaviour
{
    [Header("Hold Skill")]
    [SerializeField] private float holdSkillInterval = 0.15f;
    [SerializeField] private CharacterData characterData;

    private PlayerEvents _events;
    private Camera _mainCamera;
    private float _lastSkillRequestTime;
    private bool _skillHeld;

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

        if (isHold && _skillHeld)
        {
            if (Time.time - _lastSkillRequestTime >= holdSkillInterval)
            {
                _lastSkillRequestTime = Time.time;
                RequestSkill();
            }
        }

        if (!isHold)
        {
            _skillHeld = false;
        }
    }

    public void OnAttack(InputValue value)
    {
        if (!isOwned) return;

        if (value.isPressed)
        {
            _skillHeld = true;
            _lastSkillRequestTime = Time.time;
            RequestSkill();
        }
    }

    private void RequestSkill()
    {
        Vector3 skillDirection = GetSkillDirection();
        _events?.RequestSkill(skillDirection);
    }

    private Vector3 GetSkillDirection()
    {
        if (_mainCamera == null || Mouse.current == null)
            return transform.forward;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, characterData.GroundLayerMask))
        {
            Vector3 direction = hit.point - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
                return direction.normalized;
        }

        return transform.forward;
    }
}
