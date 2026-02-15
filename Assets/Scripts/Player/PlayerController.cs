using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;

    private Rigidbody rb;
    private PlayerInput playerInput;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = false;
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        playerInput.enabled = true;
        transform.rotation = Quaternion.identity;

        var cam = FindFirstObjectByType<QuarterViewCamera>();
        if (cam != null)
            cam.SetTarget(transform);
    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();
        playerInput.enabled = false;
    }

    void FixedUpdate()
    {
        if (!isOwned) return;

        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y).normalized * moveSpeed;
        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}
