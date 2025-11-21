using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float sprintMultiplier = 2f;

    public float mouseSensitivity = 2f;
    public float maxLookAngle = 85f;

    private float rotationX;
    private float rotationY;

    void Start()
    {
        // Lock cursor to the game window
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
    }

    void HandleMovement()
    {
        float speed = moveSpeed;

        // Hold Shift to move faster
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= sprintMultiplier;
        }

        Vector3 direction = Vector3.zero;

        // WASD
        if (Input.GetKey(KeyCode.W)) direction += transform.forward;
        if (Input.GetKey(KeyCode.S)) direction -= transform.forward;
        if (Input.GetKey(KeyCode.A)) direction -= transform.right;
        if (Input.GetKey(KeyCode.D)) direction += transform.right;

        // Up / Down (Space / Ctrl)
        if (Input.GetKey(KeyCode.Space)) direction += transform.up;
        if (Input.GetKey(KeyCode.LeftControl)) direction -= transform.up;

        transform.position += direction * speed * Time.deltaTime;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationY += mouseX;
        rotationX -= mouseY;

        rotationX = Mathf.Clamp(rotationX, -maxLookAngle, maxLookAngle);

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }
}
