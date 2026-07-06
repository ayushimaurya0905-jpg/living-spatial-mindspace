using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    private CharacterController controller;
    private float verticalLookRotation = 0f;

    // Cooldown timer — after edit panel closes, this counts down
    // before Escape-to-menu becomes active again.
    // Prevents the same Escape keypress from both closing the
    // edit panel AND immediately loading the main menu.
    private float escCooldown = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // While edit panel is open — freeze everything AND
        // keep resetting the cooldown timer so it never counts
        // down while editing is still happening.
        if (CardEditUI.IsOpen)
        {
            escCooldown = 0.15f; // reset each frame while open
            return;
        }

        // Count the cooldown down after edit panel closes.
        // During this window, Escape does nothing for this script —
        // the same keypress that closed the panel is safely ignored.
        if (escCooldown > 0f)
        {
            escCooldown -= Time.deltaTime;
            HandleMouseLook();
            HandleMovement();
            return;
        }

        HandleMouseLook();
        HandleMovement();

        // Only reach here when edit panel has been closed for
        // at least 0.15 seconds — safe to check Escape for menu.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(0);
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 80f);
        cameraTransform.localRotation =
            Quaternion.Euler(verticalLookRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical   = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * horizontal
                      + transform.forward * vertical) * moveSpeed;
        controller.SimpleMove(move);
    }
}