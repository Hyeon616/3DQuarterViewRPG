using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class PlayerAttackController : NetworkBehaviour
{
    [Header("Hold Skill")]
    [SerializeField] private float holdSkillInterval = 0.15f;

    private PlayerController _player;
    private float _lastSkillRequestTime;
    private bool _skillHeld;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
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
        _player.Events?.RequestSkill(skillDirection);
    }

    private Vector3 GetSkillDirection()
    {
        var mainCamera = _player.MainCamera;
        var characterData = _player.CharacterData;

        if (mainCamera == null || Mouse.current == null || characterData == null)
            return transform.forward;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

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
