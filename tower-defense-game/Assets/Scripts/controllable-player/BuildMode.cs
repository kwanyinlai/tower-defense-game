using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class BuildMode : MonoBehaviour
{
    private GameObject structureParentClass;
    public List<GameObject> allStructuresPrefabs = new List<GameObject>();

    public bool isBuilding = false;
    [SerializeField] private GameObject selectedBuilding;
    private GameObject buildMenu;
    public bool buildMenuOpen;

    [Header("Building Prefabs")]
    public GameObject barracksPrefab;


    void Start(){
        buildMenu = GameObject.Find("BuildMenu");
        buildMenu.SetActive(false);
        buildMenuOpen = false;
        isBuilding=false;
        structureParentClass=GameObject.Find("structures");
    }
    
    void Update()
    {

        if(!gameObject.GetComponent<TroopManagment>().managingTroops){
            if(isBuilding){
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    isBuilding = false;
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    PlaceBuilding();
                }
            } else if (buildMenuOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    isBuilding = false;
                    CloseBuildMenu();
                }
            }
            else{
                if (Input.GetKeyDown(KeyCode.B))
                {
                    OpenBuildMenu();
                }

            }
        }
       
    }

    public void PlaceBuilding() // call the outline
    {
        
        Placeable placeable = selectedBuilding.GetComponent<Placeable>();


        Vector3 placementPosition = selectedBuilding.transform.position;
        Debug.Log("Doing It For Place Building at : " + placementPosition.x + " : " + placementPosition.y + " :" + placementPosition.z);
        if (placeable.IsBuildable(selectedBuilding.transform.position) &&
            ResourcePool.EnoughResources(placeable.RequiredResources))
        {
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
            Debug.Log("not buildable? becaues\nIsBuildable => " + placeable.IsBuildable(selectedBuilding.transform.position) + "\nResources => " + ResourcePool.EnoughResources(placeable.RequiredResources));
            
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

    public void setActiveBuilding(int selection)
    {
        selectedBuilding = allStructuresPrefabs[selection];
    }
    
}
