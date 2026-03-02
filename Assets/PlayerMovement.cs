using UnityEngine;

public class SimplePlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Get WASD input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Create movement direction
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Move player
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Apply gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // small downward force to keep grounded
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}