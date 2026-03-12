using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class PlayerMoveController : NetworkBehaviour, IMovement
{
    [SerializeField] private float rotationSpeed = 10f;

    private PlayerController _player;
    private NavMeshAgent _agent;
    private Rigidbody _rigidbody;
    private Vector3 _destination;

    public NavMeshAgent Agent => _agent;
    public Vector3 Velocity => _rigidbody != null ? _rigidbody.velocity : Vector3.zero;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        CacheComponents();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        CacheComponents();
    }

    private void CacheComponents()
    {
        _agent = _player.Agent;
        _rigidbody = _player.Rigidbody;
    }

    public void OnMove(InputValue value)
    {
        if (!isOwned || _player.MainCamera == null) return;
        if (_player.AttackState != null && _player.AttackState.IsAttacking) return;

        RequestMove();
    }

    private void Update()
    {
        UpdateClient();
        UpdateServer();
    }

    private void UpdateClient()
    {
        if (!isOwned || _player.MainCamera == null) return;
        if (_player.AttackState != null && _player.AttackState.IsAttacking) return;

        bool isHold = Mouse.current != null && Mouse.current.rightButton.isPressed;
        if (isHold)
        {
            RequestMove();
        }
    }

    private void RequestMove()
    {
        if (Mouse.current == null || _player.CharacterData == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = _player.MainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _player.CharacterData.GroundLayerMask))
        {
            CmdMove(hit.point);
        }
    }

    [Command]
    private void CmdMove(Vector3 destination)
    {
        if (_agent == null) return;

        float distance = Vector3.Distance(transform.position, destination);
        if (distance < _agent.stoppingDistance + 0.1f) return;

        if (NavMesh.SamplePosition(destination, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
        {
            _destination = navHit.position;
            _agent.SetDestination(_destination);
            _player.Events.RequestMove(_destination);
        }
    }

    private void FixedUpdate()
    {
        if (!isServer) return;

        SyncAgentPosition();

        if (_player.AttackState != null && _player.AttackState.IsAttacking) return;

        MoveWithPhysics();
        FaceMovementDirection();
    }

    [Server]
    private void SyncAgentPosition()
    {
        if (_agent != null)
            _agent.nextPosition = transform.position;
    }

    [Server]
    private void MoveWithPhysics()
    {
        if (_agent == null || !_agent.hasPath)
        {
            if (_rigidbody != null)
                _rigidbody.velocity = Vector3.zero;
            return;
        }

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

        if (_player.AttackState != null && _player.AttackState.IsAttacking)
        {
            if (_agent != null && _agent.hasPath)
            {
                ResetPath();
            }
        }
    }

    [Server]
    private void FaceMovementDirection()
    {
        if (_rigidbody == null) return;

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
            if (_rigidbody != null)
                _rigidbody.velocity = Vector3.zero;
            _player.Events.StopMove();
        }
    }
}
