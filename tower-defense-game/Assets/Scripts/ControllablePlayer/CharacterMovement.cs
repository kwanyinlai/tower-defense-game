using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float angularSpeed = 60f;
    private const float GRAVITY = -9.81f;
    private Vector3 gravCalculation;

    private CharacterController characterController;

    [SerializeField] private string movementType = "character"; // movementType is either "observer", "character" or "disabled"
    public string MovementType {private get; set;}
    public float selectionDistance = 2f;
    private CharacterCameraController cameraController;
    


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        gravCalculation = Vector3.zero;   
        cameraController = GetComponent<CharacterCameraController>();
    }

    public bool IsControllable()
    {
        return movementType == "character";
    }

    void Update()
    {
        if (movementType == "character")
        {
            Character();
        }
        else if (movementType == "observer")
        {
            cameraController.Observer();
        }
        else if (movementType == "disabled")
        {
            ;
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

        Vector3 forward = cameraController.OverheadCam.transform.forward;
        Vector3 right = cameraController.OverheadCam.transform.right;

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
            Quaternion rotateDir = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateDir, angularSpeed * Time.deltaTime);
        }


    }

    void OnMouseDown()
    {
        Debug.Log("Pressed!");
    }
}
