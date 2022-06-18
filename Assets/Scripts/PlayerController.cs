using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // References
    public Transform cameraTransform;
    public CharacterController characterController;

    // Player settings
    public float cameraSensitivity;
    public float moveSpeed;
    public float moveInputDeadZone;

    // Ground detection for gravity
    public Transform groundCheck;
    public float groundDistance;
    public LayerMask walkableLayers;

    //---
    // horizontal rotation speed
    public float horizontalSpeed = 1f;
    // vertical rotation speed
    public float verticalSpeed = 1f;
    private float xRotation = 0.0f;
    private float yRotation = 0.0f;
    private Camera cam;

    public float gravity = 9.8f;
    public float velocityY = 0;

    //---

    void Start()
    {
        //---
        cam = Camera.main;
        characterController = GetComponent<CharacterController>();
        //---

    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * horizontalSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSpeed;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        cam.transform.eulerAngles = new Vector3(xRotation, yRotation, 0.0f);



        // player movement - forward, backward, left, right
        float horizontal = Input.GetAxis("Horizontal") * moveSpeed;
        float vertical = Input.GetAxis("Vertical") * moveSpeed;
        characterController.Move((cam.transform.right * horizontal + cam.transform.forward * vertical) * Time.deltaTime);
        // Gravity
        if (characterController.isGrounded)
        {
            velocityY = 0;
        }
        else
        {
            velocityY -= gravity * Time.deltaTime;
            characterController.Move(new Vector3(0, velocityY, 0));
        }
    }
}