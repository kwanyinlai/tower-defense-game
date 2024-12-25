using UnityEngine;
using UnityEngine.AI;

public class ControllableCharacter : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float angularSpeed = 700f;
    public float acceleration = 8f;
    private NavMeshAgent agent;
    public Camera playerCamera;    // Reference to the camera
    
    
    void Start()
    {
        
        agent = GetComponent<NavMeshAgent>();
        
        // Set agent settings (optional for fine-tuning movement)
        agent.speed = moveSpeed;
        agent.angularSpeed = angularSpeed;  // Set to a reasonable value for smooth rotation
        agent.acceleration = acceleration;    // Set to a reasonable value for smooth acceleration
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
                // If there's no input, return early
        if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f))
            return;

        // Calculate the forward and right directions relative to the camera
        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;

        // Flatten them on the Y-axis (we don't want the character to move up/down)
        forward.y = 0f;
        right.y = 0f;

        // Normalize to avoid faster diagonal movement
        forward.Normalize();
        right.Normalize();

        // Calculate movement direction based on input
        Vector3 moveDirection = forward * vertical + right * horizontal;

        // Only move if there's a valid direction
        if (moveDirection.magnitude >= 0.1f)
        {
            // Set the destination for the NavMeshAgent (or move the character)
            Vector3 targetPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;
            agent.SetDestination(targetPosition);

            // Optional: Rotate the player to face the direction of movement
            if (moveDirection != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 720f * Time.deltaTime);
            }
        }
    }
}
