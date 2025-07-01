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

    private CinemachineVirtualCamera overhead;
    [SerializeField] private GameObject overheadCam;

    private CinemachineVirtualCamera above;
    [SerializeField] private GameObject aboveCam;

    private CinemachineVirtualCamera wide;
    [SerializeField] private GameObject wideCam;

    private CinemachineVirtualCamera build;
    [SerializeField] private GameObject buildCam;

    public float selectionDistance = 2f;

    private GameObject currentActiveCam;

    void Start()
    {

        characterController = GetComponent<CharacterController>();
        gravCalculation = Vector3.zero;
        overhead = overheadCam.GetComponent<CinemachineVirtualCamera>();
        wide = wideCam.GetComponent<CinemachineVirtualCamera>();
        above = aboveCam.GetComponent<CinemachineVirtualCamera>();
        overheadCam.SetActive(true);
        aboveCam.SetActive(false);
        wideCam.SetActive(false);
        buildCam.SetActive(false);
        currentActiveCam = overheadCam;
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
            currentActiveCam = overheadCam;
        }
        else if (cameraState == 1)
        {
            movementType = "observer";
            overheadCam.SetActive(false);
            aboveCam.SetActive(true);
            wideCam.SetActive(false);
            currentActiveCam = overheadCam;
        }
        else
        {
            movementType = "observer";

            overheadCam.SetActive(false);
            aboveCam.SetActive(false);
            wideCam.SetActive(true);
            currentActiveCam = wideCam;
        }


        if (movementType == "character")
        {
            Character();
        }
        else if (movementType == "observer")
        {
            Debug.Log("observer");
            Observer();
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
        Vector3 movement = dir * moveSpeed;
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
        buildCam.SetActive(true);
        currentActiveCam.SetActive(false);
    }

    public void DeactivateBuildCam()
    {
        currentActiveCam.SetActive(true);
        buildCam.SetActive(false);

    }




}
