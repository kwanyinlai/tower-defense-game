using System.Collections.Generic;
using NUnit.Framework;
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
    

    private CharacterMovement camScript;



    void Start(){
        buildMenu = GameObject.Find("BuildMenu");
        buildMenu.SetActive(false);
        buildMenuOpen = false;
        isBuilding=false;
        structureParentClass=GameObject.Find("structures");
        structureOutlineParentClass=GameObject.Find("structure-outlines");
        camScript = GetComponent<CharacterMovement>();
    }
    
    void Update()
    {

        if(!gameObject.GetComponent<TroopManagment>().managingTroops){
            if(isBuilding){
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    isBuilding = false;
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
        
        Placeable placeable = selectedBuilding.GetComponent<Placeable>();
        if (placeable.IsBuildable(selectedBuilding.transform.position) &&
            ResourcePool.EnoughResources(placeable.RequiredResources))
        {
            placeable.DebugStatement();
            GameObject building = Instantiate(placeable.prefab, 
                            selectedBuilding.transform.position, 
                            selectedBuilding.transform.rotation,
                            structureParentClass.transform);
            GridSystem.OccupyArea(selectedBuilding.transform.position, placeable.size, 
                            building.GetComponent<Building>().range);
            ResourcePool.DepleteResource(placeable.RequiredResources);
        }
        else
        {
            Debug.Log("not buildable? reason :" + (placeable.IsBuildable(selectedBuilding.transform.position) ? "" : " No Buildable Space") + (ResourcePool.EnoughResources(placeable.RequiredResources) ? "" : " Not Enough Resources"));
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
        GameObject old = selectedBuilding;
        selectedBuilding = Instantiate(allStructuresPrefabs[selection], structureOutlineParentClass.transform);
        selectedBuilding.GetComponent<SnapToGrid>().InitSnapToGrid(terrain, gameObject, null);
        selectedBuilding.GetComponent<Placeable>().InitPlaceable();
        if(old != null)
        {
            Destroy(old);
        }
        camScript.ActivateBuildCam();
    }
}
