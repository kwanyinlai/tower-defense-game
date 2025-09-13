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

    private CinemachineCamera overhead;
    [SerializeField] private GameObject overheadCam;

    private CinemachineCamera above;
    [SerializeField] private GameObject aboveCam;

    private CinemachineCamera wide;
    [SerializeField] private GameObject wideCam;

    private CinemachineCamera build;
    [SerializeField] private GameObject buildCam;

    public float selectionDistance = 2f;

    private int currentActiveCam;

    private BuildMode buildScript;

    void Start()
    {

        characterController = GetComponent<CharacterController>();
        gravCalculation = Vector3.zero;
        overhead = overheadCam.GetComponent<CinemachineCamera>();
        wide = wideCam.GetComponent<CinemachineCamera>();
        above = aboveCam.GetComponent<CinemachineCamera>();
        overheadCam.SetActive(true);
        aboveCam.SetActive(false);
        wideCam.SetActive(false);
        buildCam.SetActive(false);

        buildScript = GetComponent<BuildMode>();
    }

    public bool IsControllable()
    {
        return movementType == "character";
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
            movementType = "character";
            overheadCam.SetActive(true);
            aboveCam.SetActive(false);
            wideCam.SetActive(false);
            buildCam.SetActive(false);
            currentActiveCam = 0;

        }
        else if (cameraState == 1)
        {
            movementType = "observer";
            overheadCam.SetActive(false);
            aboveCam.SetActive(true);
            wideCam.SetActive(false);
            buildCam.SetActive(false);
            currentActiveCam = 1;

        }
        else if (cameraState == 2)
        {
            movementType = "observer";
            overheadCam.SetActive(false);
            aboveCam.SetActive(false);
            wideCam.SetActive(true);
            buildCam.SetActive(false);
            currentActiveCam = 2;
        }
        else if (cameraState == 3)
        {
            movementType = "disabled";
            overheadCam.SetActive(false);
            aboveCam.SetActive(false);
            wideCam.SetActive(false);
            buildCam.SetActive(true);
        }



        if (movementType == "character")
        {
            Character();
        }
        else if (movementType == "observer")
        {
            Debug.Log("Movement Type: observer");
            Observer();
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

    void Observer()
    {
        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        Vector3 forward = overheadCam.transform.forward;
        Vector3 right = overheadCam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();


        Vector3 dir = forward * vert + right * horiz;
        dir.Normalize();
        dir.y = 0;
        Vector3 movement = dir * camSpeed;
        aboveCam.transform.Translate(movement * Time.deltaTime, Space.World);


    }

    void Character()
    {

        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        Vector3 forward = overheadCam.transform.forward;
        Vector3 right = overheadCam.transform.right;

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

    public void ActivateBuildCam()
    {
        cameraState = 3;

    }

    public void DeactivateBuildCam()
    {
        cameraState = currentActiveCam;

    }





}
