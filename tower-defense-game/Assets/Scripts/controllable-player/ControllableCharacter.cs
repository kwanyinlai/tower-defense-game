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

   
    
}
