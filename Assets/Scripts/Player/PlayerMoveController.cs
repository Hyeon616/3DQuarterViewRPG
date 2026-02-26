using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMoveController : NetworkBehaviour, IMovement
{
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private LayerMask groundLayerMask;

    private NavMeshAgent _agent;
    private PlayerInput _playerInput;
    private Camera _mainCamera;
    private IAttackState _attackState;
    private PlayerEvents _events;

    public NavMeshAgent Agent => _agent;
    public Vector3 Velocity => _agent != null ? _agent.velocity : Vector3.zero;
    public PlayerEvents Events => _events;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _playerInput = GetComponent<PlayerInput>();
        _attackState = GetComponent<IAttackState>();
        _playerInput.enabled = false;
        _events = new PlayerEvents();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _agent.enabled = true;
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

        var cam = FindFirstObjectByType<QuarterViewCamera>();
        if (cam != null)
            cam.SetTarget(transform);
    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();
        _playerInput.enabled = false;
    }

    public void OnMove(InputValue value)
    {
        if (!isOwned || _mainCamera == null) return;
        if (_attackState != null && _attackState.IsAttacking) return;

        RequestMove();
    }

    private void Update()
    {
        UpdateClient();
        UpdateServer();
    }

    private void UpdateClient()
    {
        if (!isOwned || _mainCamera == null) return;
        if (_attackState != null && _attackState.IsAttacking) return;

        bool isHold = Mouse.current != null && Mouse.current.rightButton.isPressed;
        if (isHold)
        {
            RequestMove();
        }
    }

    private void RequestMove()
    {
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayerMask))
        {
            CmdMove(hit.point);
        }
    }

    [Command]
    private void CmdMove(Vector3 destination)
    {
        float distance = Vector3.Distance(transform.position, destination);
        if (distance < _agent.stoppingDistance + 0.1f) return;

        if (NavMesh.SamplePosition(destination, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
        {
            _agent.SetDestination(navHit.position);
            _events.RequestMove(navHit.position);
        }
    }

    private void FixedUpdate()
    {
        if (!isServer) return;
        if (_attackState != null && _attackState.IsAttacking) return;

        FaceMovementDirection();
    }

    private void UpdateServer()
    {
        if (!isServer) return;

        if (_attackState != null && _attackState.IsAttacking)
        {
            if (_agent.hasPath)
            {
                ResetPath();
            }
        }
    }

    [Server]
    private void FaceMovementDirection()
    {
        Vector3 horizontalVelocity = new Vector3(_agent.velocity.x, 0f, _agent.velocity.z);

        if (horizontalVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    public void Move(Vector3 delta)
    {
        if (_agent != null && _agent.enabled)
        {
            _agent.Move(delta);
        }
    }

    public void ResetPath()
    {
        if (_agent != null && _agent.hasPath)
        {
            _agent.ResetPath();
            _events.StopMove();
        }
    }
}