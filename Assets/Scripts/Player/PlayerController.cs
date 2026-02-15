using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float rotationSpeed = 10f;

    private NavMeshAgent agent;
    private PlayerInput playerInput;
    private Camera mainCamera;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        playerInput = GetComponent<PlayerInput>();
        agent.enabled = false;
        playerInput.enabled = false;
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        agent.enabled = true;
        playerInput.enabled = true;
        transform.rotation = Quaternion.identity;
        mainCamera = Camera.main;

        var cam = FindFirstObjectByType<QuarterViewCamera>();
        if (cam != null)
            cam.SetTarget(transform);
    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();
        agent.enabled = false;
        playerInput.enabled = false;
    }

    void Update()
    {
        if (!isOwned) return;
        FaceTarget();
    }

    public void OnMove(InputValue value)
    {
        if (!isOwned || mainCamera == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            agent.SetDestination(hit.point);
        }
    }

    private void FaceTarget()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
