using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    public float walkSpeed = 5f; //how fast they walk
    public float runSpeed = 10f; // how fast they run
    public float jumpPower = 7f;  //jump height
    public float gravity = -15f; // Stadard gravity
    private Vector3 velocity;
    private float speed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        speed = walkSpeed;
    }

    void Update()
    {
        // Handle the Movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Use transform.right and transform.forward to move relative to player's rotation
        Vector3 moveDirection = transform.right * horizontalInput + transform.forward * verticalInput;

        // Apply movement
        controller.Move(moveDirection * speed * Time.deltaTime);

        // Handle Gravity and Jumping
        if (controller.isGrounded)
        {
            // Reset vertical velocity when on the ground
            velocity.y = -2f;

            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpPower * -2f * gravity);
            }
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Apply vertical movement (jumping/falling)
        controller.Move(velocity * Time.deltaTime);

        // player can sprint 
        if (Input.GetKeyDown(KeyCode.LeftShift))
            speed = runSpeed;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            speed = walkSpeed;
    }
}
