using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private SimplePlayerStun stunComponent; // Reference to the stun script

    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpPower = 7f;
    public float gravity = -15f;
    private Vector3 velocity;
    private float speed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        speed = walkSpeed;

        // Automatically find the stun script on the same object
        stunComponent = GetComponent<SimplePlayerStun>();
    }

    void Update()
    {
        // 1. STUN CHECK: If stunned, stop horizontal movement and skip input
        if (stunComponent != null && stunComponent.isStunned)
        {
            // Zero out movement velocity so they don't slide
            velocity.x = 0;
            velocity.z = 0;

            // Still apply gravity so they fall if stunned in mid-air
            if (!controller.isGrounded)
            {
                velocity.y += gravity * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime);
            }

            return; // EXIT early so the player cannot move or jump
        }

        // --- NORMAL MOVEMENT LOGIC ---
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * horizontalInput + transform.forward * verticalInput;
        controller.Move(moveDirection * speed * Time.deltaTime);

        if (controller.isGrounded)
        {
            velocity.y = -2f;
            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpPower * -2f * gravity);
            }
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.LeftShift))
            speed = runSpeed;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            speed = walkSpeed;
    }
}