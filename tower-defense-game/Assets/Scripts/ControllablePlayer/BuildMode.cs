using System.Collections.Generic;
using UnityEngine;

public class BuildMode : MonoBehaviour
{
    // Building Parents (for organization)
    [SerializeField] private GameObject buildingStructureParentClass
    ;
    [SerializeField] private GameObject buildingStructureOutlineParentClass
    ;


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



    void Start(){
        buildMenu = BuildMenu.Instance.gameObject;
        buildMenu.SetActive(false);
        buildMenuOpen = false;
        isPlacingBuilding=false;
        cameraController = GetComponent<CharacterCameraController>();
        gridManager = GridManager.Instance;
    }
    
    void Update()
    {

        if(!gameObject.GetComponent<TroopManagment>().IsManagingTroops){
            if(isPlacingBuilding){
                
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    isPlacingBuilding = false;
                    if (outline != null)
                    {
                        Destroy(outline);
                    }
                    cameraController.DeactivateBuildCam();

                    gridManager.ClearBuildGrid();
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    PlaceBuilding();
                }
                else if(Input.GetKeyDown(KeyCode.R))
                {
                    outline.transform.Rotate(0, 90, 0);
                    Debug.Log("rotated"); 
                }
            }
            else if (buildMenuOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    buildMenuOpen = false;
                    CloseBuildMenu();
                    cameraController.DeactivateBuildCam();
                    isPlacingBuilding = false;
                }
            }
            else
                if (Input.GetKeyDown(KeyCode.B))
                {
                    OpenBuildMenu();
                }

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
        buildMenuOpen = true;
    }
    
    public void CloseBuildMenu()
    {
        buildMenu.SetActive(false);
        buildMenuOpen = false;
        isPlacingBuilding = true;
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
