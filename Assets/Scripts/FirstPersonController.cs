using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public Transform cameraTransform; // drag the child Camera here in the Inspector

    private CharacterController controller;
    private float verticalLookRotation = 0f; // tracks how far we've tilted up/down

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Lock the cursor to the middle of the screen and hide it,
        // so mouse movement controls looking instead of dragging a pointer around.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();

        // Escape frees the cursor again — handy for testing, and we'll reuse this for menus later.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Turning left/right rotates the WHOLE player body, so the direction you walk turns too.
        transform.Rotate(Vector3.up * mouseX);

        // Looking up/down only tilts the camera — not the body.
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 80f); // stop before flipping upside down
        cameraTransform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float vertical = Input.GetAxis("Vertical");     // W/S or Up/Down

        // transform.forward/right always point the way the body is CURRENTLY facing,
        // so "forward" means forward-for-you, not a fixed world direction.
        Vector3 move = (transform.right * horizontal + transform.forward * vertical) * moveSpeed;

        controller.SimpleMove(move); // SimpleMove applies gravity for us automatically
    }
}