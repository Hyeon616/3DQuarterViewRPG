using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMoveController : NetworkBehaviour, IMovement
{
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private LayerMask groundLayerMask;

    private NavMeshAgent _agent;
    private Rigidbody _rigidbody;
    private PlayerInput _playerInput;
    private Camera _mainCamera;
    private IAttackState _attackState;
    private PlayerEvents _events;
    private Vector3 _destination;

    public NavMeshAgent Agent => _agent;
    public Vector3 Velocity => _rigidbody != null ? _rigidbody.velocity : Vector3.zero;
    public PlayerEvents Events => _events;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _rigidbody = GetComponent<Rigidbody>();
        _playerInput = GetComponent<PlayerInput>();
        _attackState = GetComponent<IAttackState>();
        _playerInput.enabled = false;
        _events = new PlayerEvents();

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
            _destination = navHit.position;
            _agent.SetDestination(_destination);
            _events.RequestMove(_destination);
        }
    }

    private void FixedUpdate()
    {
        if (!isServer) return;

        SyncAgentPosition();

        if (_attackState != null && _attackState.IsAttacking) return;

        MoveWithPhysics();
        FaceMovementDirection();
    }

    [Server]
    private void SyncAgentPosition()
    {
        _agent.nextPosition = transform.position;
    }

    [Server]
    private void MoveWithPhysics()
    {
        if (!_agent.hasPath)
        {
            _rigidbody.velocity = Vector3.zero;
            return;
        }

        // 목적지 근처에 도달하면 멈춤 (수평 거리만 체크)
        Vector3 toDestination = _destination - transform.position;
        toDestination.y = 0f;
        if (toDestination.magnitude <= _agent.stoppingDistance)
        {
            _rigidbody.velocity = Vector3.zero;
            ResetPath();
            return;
        }

        Vector3 desiredVelocity = _agent.desiredVelocity;
        desiredVelocity.y = 0f;

        _rigidbody.velocity = desiredVelocity;
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
        Vector3 horizontalVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);

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
            _rigidbody.velocity = Vector3.zero;
            _events.StopMove();
        }
    }
}