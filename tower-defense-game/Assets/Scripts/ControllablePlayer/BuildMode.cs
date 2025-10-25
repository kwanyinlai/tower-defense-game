using System.Collections.Generic;
using UnityEngine;

public class BuildMode : MonoBehaviour
{
    // Building Parents (for organization)
    [SerializeField] private GameObject buildingStructureParentClass
    ;
    [SerializeField]
    private GameObject buildingStructureOutlineParentClass
    ;

    private Player playerData;


    // Asdf
    private GameObject buildMenu;
    private GameObject outline;

    //
    [SerializeField] private GameObject terrain;
    [SerializeField] private GameObject selectedBuilding;

    [Header("Building Prefabs")]
    public List<GameObject> allStructuresPrefabs = new List<GameObject>();
    public GameObject barracksPrefab;


    // Reference Scripts
    private CharacterCameraController cameraController;
    private GridManager gridManager;



    void Start() {
        buildMenu = BuildMenu.Instance.gameObject;
        buildMenu.SetActive(false);
        playerData = GetComponent<Player>();
        cameraController = GetComponent<CharacterCameraController>();
        gridManager = GridManager.Instance;
    }

    void Update()
    {
        DetermineState();

        if (playerData.CurrentState == Player.PlayerStates.PlacingBuilding)
        {
            PlayerDecidingBuildingPlacement();
        }
        else if (playerData.CurrentState == Player.PlayerStates.BuildMenuOpen)
        {
            ;
        }
        else if (playerData.CurrentState == Player.PlayerStates.ControllingCharacter)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                OpenBuildMenu();
            }
            else
            {
                CloseBuildMenu();
            }
        }

    }

    void StopPlacingBuilding()
    {
        playerData.CurrentState = Player.PlayerStates.ControllingCharacter;
        if (outline != null)
        {
            Destroy(outline);
        }
        cameraController.DeactivateBuildCam();

        gridManager.ClearBuildGrid();
    }

    private void DetermineState()
    {
        if (playerData.CurrentState == Player.PlayerStates.PlacingBuilding)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                playerData.CurrentState = Player.PlayerStates.ControllingCharacter;
            }
        }
        else if (playerData.CurrentState == Player.PlayerStates.BuildMenuOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                playerData.CurrentState = Player.PlayerStates.ControllingCharacter; 
                cameraController.DeactivateBuildCam();
            }
        }
    }

    void PlayerDecidingBuildingPlacement()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopPlacingBuilding();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            PlaceBuilding();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            outline.transform.Rotate(0, 90, 0);
            Debug.Log("rotated");
        }
    }





    public void PlaceBuilding() // call the outline
    {

        Placeable placeable = outline.GetComponent<Placeable>();
        if (placeable.IsBuildable(outline.transform.position) &&
            ResourcePool.EnoughResources(placeable.RequiredResources))
        {
            placeable.DebugStatement();
            GameObject building = Instantiate(placeable.prefab,
                            outline.transform.position,
                            outline.transform.rotation,
                            buildingStructureParentClass
                            .transform);
            gridManager.OccupyArea(outline.transform.position, placeable.size,
                            building.GetComponent<Building>().range);
            ResourcePool.DepleteResource(placeable.RequiredResources);
        }
        else
        {
            Debug.Log("not buildable? reason :" + (placeable.IsBuildable(outline.transform.position) ? "" : " No Buildable Space") + (ResourcePool.EnoughResources(placeable.RequiredResources) ? "" : " Not Enough Resources"));
        }
    }



    public void OpenBuildMenu()
    {
        buildMenu.SetActive(true);
        playerData.CurrentState = Player.PlayerStates.BuildMenuOpen;
    }

    public void CloseBuildMenu()
    {
        buildMenu.SetActive(false);   
        // playerData.currentState = PlayerStates.PlacingBuilding;
        // gridManager.DrawBuildGrid();
        // TODO: check whether I need to call this in CloseBuildMenu or else where
    }
    
    void StartBuilding()
    {
        gridManager.DrawBuildGrid();
        playerData.CurrentState = Player.PlayerStates.PlacingBuilding;
        gridManager.DrawBuildGrid();
    }

    public void SetActiveBuilding(int selection)
    {
        selectedBuilding = allStructuresPrefabs[selection];

        outline = Instantiate(selectedBuilding, buildingStructureOutlineParentClass
        .transform);

        outline.GetComponent<SnapToGrid>().InitSnapToGrid(terrain, gameObject);
        outline.GetComponent<Placeable>().InitPlaceable();
    
        cameraController.ActivateBuildCam();
    }
}
