using UnityEngine;
using System.Collections.Generic;

public class CharacterMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Movement speed
    public float angularSpeed = 700f; // Rotation speed
    public Transform cam;
    

    

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        Movement();

    
        
        
    }

    void Movement(){
        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        Vector3 forward = cam.forward;
        Vector3 right = cam.right;


        
        forward.Normalize();
        right.Normalize();

        
        Vector3 dir = forward * vert + right * horiz;
        dir.y = 0;
        dir.Normalize();

        
       

        
        
        characterController.Move(dir * moveSpeed * Time.deltaTime );

        
        if (dir != Vector3.zero)
        {
            Quaternion rotateDir = Quaternion.LookRotation(-1*dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateDir, angularSpeed * Time.deltaTime);
        }
        
        

    }
}
