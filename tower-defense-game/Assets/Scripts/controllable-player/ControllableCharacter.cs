using UnityEngine;
using UnityEngine.AI;

public class ControllableCharacter : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float angularSpeed = 700f;
    public float acceleration = 8f;
    private NavMeshAgent agent;
    public Camera playerCamera; 
    
    void Start()
    {
        
        agent = GetComponent<NavMeshAgent>();
        
        agent.speed = moveSpeed;
        agent.angularSpeed = angularSpeed;
        agent.acceleration = acceleration;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f))
            return;

       
        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;

       
        forward.y = 0f;
        right.y = 0f;

        // Normalize to avoid faster diagonal movement
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * vertical + right * horizontal;

        if (moveDirection.magnitude >= 0.1f)
        {
            
            Vector3 targetPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;
            agent.SetDestination(targetPosition);

            
            if (moveDirection != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 720f * Time.deltaTime);
            }
        }
    }
}
