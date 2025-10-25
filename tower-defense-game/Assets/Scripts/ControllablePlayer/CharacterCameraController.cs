using UnityEngine;
using Unity.Cinemachine;

public class CharacterCameraController : MonoBehaviour
{
    private Player playerData;
    private int cameraState;

    private float camSpeed = 50f;

    private CinemachineCamera overhead;
    [SerializeField] private GameObject overheadCam;
    public Vector3 OverheadCamForward
    {
        get{ return overheadCam.transform.forward; }
    }
    public Vector3 OverheadCamRight
    {
        get{ return overheadCam.transform.right; }
    }

    private CinemachineCamera above;
    [SerializeField] private GameObject aboveCam;

    private CinemachineCamera wide;
    [SerializeField] private GameObject wideCam;

    private CinemachineCamera build;
    [SerializeField] private GameObject buildCam;

    private int currentActiveCam;


    private CharacterMovement movementController;

    void Start()
    {
        overhead = overheadCam.GetComponent<CinemachineCamera>();
        wide = wideCam.GetComponent<CinemachineCamera>();
        above = aboveCam.GetComponent<CinemachineCamera>();
        overheadCam.SetActive(true);
        aboveCam.SetActive(false);
        wideCam.SetActive(false);
        buildCam.SetActive(false);

        movementController = GetComponent<CharacterMovement>();
        playerData = GetComponent<Player>();
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
            playerData.CurrentState = Player.PlayerStates.ControllingCharacter;
            overheadCam.SetActive(true);
            aboveCam.SetActive(false);
            wideCam.SetActive(false);
            buildCam.SetActive(false);
            currentActiveCam = 0;

        }
        else if (cameraState == 1)
        {
            playerData.CurrentState = Player.PlayerStates.ObserverMode;
            overheadCam.SetActive(false);
            aboveCam.SetActive(true);
            wideCam.SetActive(false);
            buildCam.SetActive(false);
            currentActiveCam = 1;

        }
        else if (cameraState == 2)
        {
            playerData.CurrentState = Player.PlayerStates.DisabledControls;
            overheadCam.SetActive(false);
            aboveCam.SetActive(false);
            wideCam.SetActive(true);
            buildCam.SetActive(false);
            currentActiveCam = 2;
        }
        else if (cameraState == 3)
        {
            overheadCam.SetActive(false);
            aboveCam.SetActive(false);
            wideCam.SetActive(false);
            buildCam.SetActive(true);
        }
    }

    public void Observer()
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

    public void ActivateBuildCam()
    {
        cameraState = 3;

    }

    public void DeactivateBuildCam()
    {
        cameraState = currentActiveCam;

    }
}
