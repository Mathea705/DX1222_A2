using Unity.Netcode;
using UnityEngine;

public class FirstPersonController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckDistance = 0.1f;

    [SerializeField] private Camera playerCamera;
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private Renderer[] hideFromOwner;

    private NetworkVariable<int> playerIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerCamera.enabled = true;

            foreach (Renderer r in hideFromOwner)
            {
                if (r != null) r.enabled = false;
            }
        }
        else
        {
            playerCamera.enabled = false;
        }

        playerIndex.OnValueChanged += OnPlayerIndexChanged;
        ApplyPlayerColor();
    }

    public void SetPlayerIndex(int index)
    {
        if (IsServer)
        {
            playerIndex.Value = index;
        }
    }

    private void OnPlayerIndexChanged(int oldValue, int newValue)
    {
        ApplyPlayerColor();
    }

    private void ApplyPlayerColor()
    {

        Color playerColor = playerIndex.Value == 0 ? Color.red : Color.blue;
        playerRenderer.material.color = playerColor;
    }
    
    void Update()
    {
        if (!IsOwner) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);

        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.5f);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    
    void FixedUpdate()
    {
        if (!IsOwner) return;
        
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = transform.right * horizontal + transform.forward * vertical;
        Vector3 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
        
        rb.MovePosition(newPosition);
    }
}
