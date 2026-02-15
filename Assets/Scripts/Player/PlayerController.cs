
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public override void OnStartLocalPlayer()
    {
        playerInput.enabled = true;
        AttachCamera();
    }
    
    void FixedUpdate()
    {
        if(!isLocalPlayer)
            return;

        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        move = move.normalized * moveSpeed;

        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    
    void AttachCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) 
            return;

        cam.transform.SetParent(transform);
        cam.transform.localPosition    = new Vector3(0, 6, -9);
        cam.transform.localEulerAngles = new Vector3(25, 0, 0);
    }
}
