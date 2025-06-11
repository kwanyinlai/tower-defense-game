using UnityEngine;
using Unity.Cinemachine;

public class CharacterMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float angularSpeed = 60f;
    private const float GRAVITY = -9.81f;
    private Vector3 gravCalculation;

    private CharacterController characterController;

    [SerializeField] private string movementType = "character"; // movementType is either "observer", "character" or "disabled"

    private int cameraState;
    private Vector3 posWhole;
    private Quaternion rotWhole;
    private float camSpeed = 50f;

    [SerializeField] private Camera cam;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    void Start()
    {
        
        characterController = GetComponent<CharacterController>();
        gravCalculation = Vector3.zero;
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        virtualCamera.Follow = transform;
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.C))
        {
            cameraState++;
            cameraState %= 3;
        }
        /*
        if (cameraState == 0)
        {
            GetComponent<Camera>().orthographicSize = 20f; ;
            cam.position = posWhole;
            movementType = "character";
        }
        else if (cameraState == 1)
        {
            GetComponent<Camera>().orthographicSize = 50f;
            cam.position = transform.position + posWhole;
            movementType = "observer";
        }
        else
        {
            CameraMovement();
            movementType = "observer";
        }
        */
        if (movementType == "character")
        {
            Character();
        }
        else if (movementType == "observer")
        {
            Debug.Log("observer");
        }
        else if (movementType == "disabled")
        {
            Debug.Log("disabled");
        }
        else
        {
            Debug.Log("Invalid movement type " + movementType);
        }


    }
    
    


    void Character()
    {
        
        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        Vector3 forward = virtualCamera.transform.forward;
        Vector3 right = virtualCamera.transform.right;

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
