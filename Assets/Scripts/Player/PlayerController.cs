using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayableAnimator))]
public class PlayerController : NetworkBehaviour
{
    // Action Map 이름 상수
    public static class ActionMaps
    {
        public const string Player = "Player";
        public const string UI = "UI";
        public const string Skills = "Skills";
    }

    // 공통 컴포넌트 참조
    private NavMeshAgent _agent;
    private Rigidbody _rigidbody;
    private PlayerInput _playerInput;
    private IAnimatable _animatable;
    private CharacterData _characterData;
    private Camera _mainCamera;

    // 이벤트 시스템
    private PlayerEvents _events;

    // 하위 컨트롤러 참조
    private IAttackState _attackState;
    private IPlayerStat _playerStat;
    private PlayerSkillCooldown _skillCooldown;

    // Action Maps
    private InputActionMap _playerMap;
    private InputActionMap _uiMap;
    private InputActionMap _skillsMap;

    // 공개 프로퍼티
    public NavMeshAgent Agent => _agent;
    public Rigidbody Rigidbody => _rigidbody;
    public PlayerInput PlayerInput => _playerInput;
    public IAnimatable Animatable => _animatable;
    public CharacterData CharacterData => _characterData;
    public Camera MainCamera => _mainCamera;
    public PlayerEvents Events => _events;
    public IAttackState AttackState => _attackState;
    public IPlayerStat PlayerStat => _playerStat;
    public PlayerSkillCooldown SkillCooldown => _skillCooldown;

    private void Awake()
    {
        // 공통 컴포넌트 캐싱
        _agent = GetComponent<NavMeshAgent>();
        _rigidbody = GetComponent<Rigidbody>();
        _playerInput = GetComponent<PlayerInput>();
        _animatable = GetComponent<IAnimatable>();
        _characterData = GetComponent<ICharacterData>()?.CharacterData;
        _attackState = GetComponent<IAttackState>();
        _playerStat = GetComponent<IPlayerStat>();
        _skillCooldown = GetComponent<PlayerSkillCooldown>();

        // 이벤트 시스템 초기화
        _events = new PlayerEvents();

        // 초기 설정
        _playerInput.enabled = false;
        SetupRigidbody();
    }

    private void SetupRigidbody()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _agent.enabled = true;
        _agent.updatePosition = false;
        _agent.updateRotation = false;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        _agent.enabled = false;
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        _playerInput.enabled = true;
        _mainCamera = Camera.main;

        InitializeActionMaps();

        var cam = FindFirstObjectByType<QuarterViewCamera>();
        if (cam != null)
            cam.SetTarget(transform);
    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();
        DisableAllActionMaps();
        _playerInput.enabled = false;
    }

    private void InitializeActionMaps()
    {
        if (_playerInput.actions == null) return;

        _playerMap = _playerInput.actions.FindActionMap(ActionMaps.Player);
        _uiMap = _playerInput.actions.FindActionMap(ActionMaps.UI);
        _skillsMap = _playerInput.actions.FindActionMap(ActionMaps.Skills);

        // 모든 맵 활성화 (동시 사용)
        _playerMap?.Enable();
        _uiMap?.Enable();
        _skillsMap?.Enable();
    }

    private void DisableAllActionMaps()
    {
        _playerMap?.Disable();
        _uiMap?.Disable();
        _skillsMap?.Disable();
    }

    /// <summary>
    /// 특정 Action Map 활성화/비활성화
    /// </summary>
    public void SetActionMapEnabled(string mapName, bool isEnabled)
    {
        var map = _playerInput.actions?.FindActionMap(mapName);
        if (map == null) return;

        if (isEnabled)
            map.Enable();
        else
            map.Disable();
    }

    /// <summary>
    /// Player 입력 일시 중지 (UI 열림 등)
    /// </summary>
    public void SetPlayerInputEnabled(bool isEnabled)
    {
        if (isEnabled)
            _playerMap?.Enable();
        else
            _playerMap?.Disable();
    }
}
