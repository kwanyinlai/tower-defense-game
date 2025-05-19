using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float angularSpeed = 60f;
    private const float GRAVITY = -9.81f;
    private Vector3 gravCalculation;

    private CharacterController characterController;

    private string movementType = "character"; // movementType is either "observer", "character" or "disabled"

    [SerializeField] private Camera camera;
    private Transform cam;

    private int cameraState;
    private Vector3 posWhole;
    private Quaternion rotWhole;
    private float camSpeed = 50f;



    void Start()
    {
        cam = camera.transform;
        characterController = GetComponent<CharacterController>();
        gravCalculation = Vector3.zero;
        posWhole = new Vector3(10f, 8f, -10f);
        rotWhole = Quaternion.Euler(new Vector3(20f, 210f, 0f));
        cam.position = posWhole;
        cam.rotation = rotWhole;
        cameraState = 0;
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.C))
        {
            cameraState++;
            cameraState %= 3;
        }

        if (cameraState == 0)
        {
            camera.orthographicSize = 20f; ;
            cam.position = posWhole;
            movementType = "character";
        }
        else if (cameraState == 1)
        {
            camera.orthographicSize = 50f;
            cam.position = transform.position + posWhole;
            movementType = "observer";
        }
        else
        {
            cameraMovement();
            movementType = "observer";
        }

        if (movementType == "character")
        {
            Character();
        }
        else if (movementType == "observer")
        {
            Debug.Log("");
        }
        else if (movementType == "disabled")
        {
            Debug.Log("");
        }
        else
        {
            Debug.Log("Invalid movement type " + movementType);
        }


    }
    
    void cameraMovement()
    {
        Vector3 moveCamera = new Vector3(0, 0, 0);
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

        moveCamera = dir * camSpeed * Time.deltaTime;
        cam.position += moveCamera;
    }


    void Character()
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
