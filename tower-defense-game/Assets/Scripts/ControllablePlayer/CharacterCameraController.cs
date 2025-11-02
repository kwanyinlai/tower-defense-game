using UnityEngine;
using Unity.Cinemachine;

public class CharacterCameraController : MonoBehaviour
{
    private PlayerManager playerData;
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
        playerData = GetComponent<PlayerManager>();
    }

    public PlayerManager.PlayerStates ChangeCameraState()
    {
        cameraState++;
        cameraState %= 3;

        if (cameraState == 0)
        {
            return PlayerManager.PlayerStates.ControllingCharacter;
        }
        else if (cameraState == 1)
        {
            return PlayerManager.PlayerStates.ObserverMode;
        }
        else if (cameraState == 2)
        {
            return PlayerManager.PlayerStates.DisabledControls;
        }
        return PlayerManager.PlayerStates.ControllingCharacter; // SHOULDN'T BE HERE
    }
    void Update()
    {

        if (playerData.CurrentState == PlayerManager.PlayerStates.PlacingBuilding)
        {
            overheadCam.SetActive(false);
            aboveCam.SetActive(false);
            wideCam.SetActive(false);
            buildCam.SetActive(true);
        }
        else
        {
            if (cameraState == 0)
            {
                overheadCam.SetActive(true);
                aboveCam.SetActive(false);
                wideCam.SetActive(false);
                buildCam.SetActive(false);
                currentActiveCam = 0;

            }
            else if (cameraState == 1)
            {
                overheadCam.SetActive(false);
                aboveCam.SetActive(true);
                wideCam.SetActive(false);
                buildCam.SetActive(false);
                currentActiveCam = 1;

            }
            else if (cameraState == 2)
            {
                overheadCam.SetActive(false);
                aboveCam.SetActive(false);
                wideCam.SetActive(true);
                buildCam.SetActive(false);
                currentActiveCam = 2;
            }
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
