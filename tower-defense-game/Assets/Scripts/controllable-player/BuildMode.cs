using System.Collections.Generic;
using UnityEngine;

public class BuildMode : MonoBehaviour
{
    // Building Parents (for organization)
    [SerializeField] private GameObject structureParentClass;
    [SerializeField] private GameObject structureOutlineParentClass;

    [Header("Build Mode Flags")]
    public bool isBuilding = false;
    public bool buildMenuOpen;

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
    private CharacterMovement camScript;
    private GridManager gridManager;



    void Start(){
        buildMenu = BuildMenu.Instance.gameObject;
        buildMenu.SetActive(false);
        buildMenuOpen = false;
        isBuilding=false;
        camScript = GetComponent<CharacterMovement>();
        gridManager = GridManager.Instance;
    }
    
    void Update()
    {

        if(!gameObject.GetComponent<TroopManagment>().managingTroops){
            if(isBuilding){
                
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    isBuilding = false;
                    if (outline != null)
                    {
                        Destroy(outline);
                    }
                    camScript.DeactivateBuildCam();

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
                    camScript.DeactivateBuildCam();
                    isBuilding = false;
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
                            structureParentClass.transform);
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
        isBuilding = true;
        gridManager.DrawBuildGrid();

    }

    public void SetActiveBuilding(int selection)
    {
        selectedBuilding = allStructuresPrefabs[selection];

        outline = Instantiate(selectedBuilding, structureOutlineParentClass.transform);

        outline.GetComponent<SnapToGrid>().InitSnapToGrid(terrain, gameObject);
        outline.GetComponent<Placeable>().InitPlaceable();
    
        camScript.ActivateBuildCam();
    }
}
