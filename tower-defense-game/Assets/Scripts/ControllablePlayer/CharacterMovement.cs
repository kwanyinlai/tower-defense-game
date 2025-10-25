using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float angularSpeed = 60f;
    private const float GRAVITY = -9.81f;
    private Vector3 gravCalculation;

    private CharacterController characterController;

    private float selectionDistance = 2f;
    private CharacterCameraController cameraController;

    private Player playerData;
    


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        gravCalculation = Vector3.zero;
        cameraController = GetComponent<CharacterCameraController>();
        playerData = GetComponent<Player>();
        
    }

    public bool IsControllable()
    {
        return playerData.CurrentState == Player.PlayerStates.ControllingCharacter;
    }

    void Update()
    {
        if (playerData.CurrentState == Player.PlayerStates.ControllingCharacter)
        {
            Character();
        }
        else if (playerData.CurrentState == Player.PlayerStates.ObserverMode)
        {
            cameraController.Observer();
        }
        else if (playerData.CurrentState == Player.PlayerStates.DisabledControls)
        {
            ;
        }
    }

    void Character()
    {

        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        Vector3 forward = GetComponent<CharacterCameraController>().OverheadCamForward;
        Vector3 right = GetComponent<CharacterCameraController>().OverheadCamRight;

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
