using UnityEngine;

public class SimpleFlyCamera : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float mouseSensitivity = 2f;
    public float zoomSpeed = 300f;

    public float minHeight = 5f;
    public float maxHeight = 100f;

    float yaw;
    float pitch;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Look();
        Move();
        Zoom();
    }

    void Look()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity * 100f * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * 100f * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, -85f, 85f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 direction = transform.forward * v + transform.right * h;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 pos = transform.position;
            pos.y -= scroll * zoomSpeed * Time.deltaTime;
            pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);
            transform.position = pos;
        }
    }
}
