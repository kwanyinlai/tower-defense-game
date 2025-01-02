using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float angularSpeed = 60f;
    public Transform cam;
    private const float GRAVITY = -9.81f;
    private Vector3 gravCalculation;

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        gravCalculation = Vector3.zero;
    }

    void Update()
    {
        Movement();
    }

    void Movement()
    {

        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

       
        Vector3 dir = forward * vert + right * horiz;
        dir.Normalize(); 


        if (characterController.isGrounded)
        {
            gravCalculation.y = -0.5f;  
        }
        else
        {
            gravCalculation.y += GRAVITY * Time.deltaTime; 
        }

        Vector3 movement = dir * moveSpeed + gravCalculation;
        characterController.Move(movement * Time.deltaTime);
    


        
        if (dir != Vector3.zero)
        {
            dir *= -1;
            Quaternion rotateDir = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateDir, angularSpeed * Time.deltaTime);
        }
    }
}
