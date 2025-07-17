using System.Collections.Generic;
using UnityEngine;

public class BuildMode : MonoBehaviour
{
    private GameObject structureParentClass;
    private GameObject structureOutlineParentClass;
    public List<GameObject> allStructuresPrefabs = new List<GameObject>();

    public bool isBuilding = false;
    [SerializeField] private GameObject selectedBuilding;
    private GameObject buildMenu;
    public bool buildMenuOpen;
    [SerializeField] private GameObject terrain;

    [Header("Building Prefabs")]
    public GameObject barracksPrefab;

    private GameObject outline;

    private CharacterMovement camScript;

    private GridSystem gridManager;



    void Start(){
        buildMenu = GameObject.Find("BuildMenu");
        buildMenu.SetActive(false);
        buildMenuOpen = false;
        isBuilding=false;
        structureParentClass=GameObject.Find("structures");
        structureOutlineParentClass=GameObject.Find("structure-outlines");
        camScript = GetComponent<CharacterMovement>();
        gridManager = GameObject.Find("grid-manager").GetComponent<GridSystem>();
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
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    PlaceBuilding();
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
