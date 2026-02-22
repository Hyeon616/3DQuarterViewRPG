using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMoveController : NetworkBehaviour
{
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private LayerMask groundLayerMask;

    private NavMeshAgent agent;
    private PlayerInput playerInput;
    private Camera mainCamera;
    private PlayerAnimationController animController;

    public NavMeshAgent Agent => agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        playerInput = GetComponent<PlayerInput>();
        animController = GetComponent<PlayerAnimationController>();
        playerInput.enabled = false;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        agent.enabled = true;
        agent.updateRotation = false;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        agent.enabled = false;
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        playerInput.enabled = true;
        mainCamera = Camera.main;

        var cam = FindFirstObjectByType<QuarterViewCamera>();
        if (cam != null)
            cam.SetTarget(transform);
    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();
        playerInput.enabled = false;
    }

    public void OnMove(InputValue value)
    {
        if (!isOwned || mainCamera == null) return;

        // 공격 중에는 이동 불가
        if (animController != null && animController.IsAttacking) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayerMask))
        {
            CmdMove(hit.point);
        }
    }

    [Command]
    private void CmdMove(Vector3 destination)
    {
        float distance = Vector3.Distance(transform.position, destination);
        if (distance < agent.stoppingDistance + 0.1f) return;

        
        if (NavMesh.SamplePosition(destination, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position); 
        }
    }

    void Update()
    {
        if (!isServer) return;

        // 공격 중이면 이동 멈춤
        if (animController != null && animController.IsAttacking)
        {
            if (agent.hasPath)
            {
                agent.ResetPath();
            }
            return;
        }

        FaceTarget();
    }

    [Server]
    private void FaceTarget()
    {
        Vector3 horizontalVelocity = new Vector3(agent.velocity.x, 0f, agent.velocity.z);

        if (horizontalVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}