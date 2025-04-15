using UnityEngine;

public class BuildMode : MonoBehaviour
{
    private GameObject structureParentClass;

    public bool isBuilding = false;
    [SerializeField] private GameObject selectedBuilding;
    public bool buildMenu;

    [Header("Building Prefabs")]
    public GameObject barracksPrefab;


    void Start(){
        buildMenu = true; 
        isBuilding=false;
        structureParentClass=GameObject.Find("structures");
    }
    
    void Update()
    {

        if(!gameObject.GetComponent<TroopManagment>().managingTroops){
            


            if(isBuilding){
               
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CloseBuildMenu();
                }
                if(buildMenu){
                    OpenBuildMenu();
                }
                else{
                    
                    if (Input.GetKeyDown(KeyCode.Return)){
                        
                        PlaceBuilding();
                        // CloseBuildMenu();
                    }
                }



            }
            else{
                if (Input.GetKeyDown(KeyCode.B))
                {
                    isBuilding = true;
                    
                }

            }
        }
       
    }

    public void PlaceBuilding() // call the outline
    {
        
        Placeable placeable = selectedBuilding.GetComponent<Placeable>();




        if (placeable.IsBuildable(selectedBuilding.transform.position) &&
            ResourcePool.EnoughResources(placeable.RequiredResources))
        {
            GameObject building = Instantiate(placeable.prefab, 
                            selectedBuilding.transform.position, 
                            selectedBuilding.transform.rotation,
                            structureParentClass.transform);
            GridSystem.OccupyArea(selectedBuilding.transform.position, placeable.size, 
                            building.GetComponent<Building>().range);
            building.GetComponent<Building>().InstantiateBuilding(placeable.RequiredResources);
            ResourcePool.DepleteResource(placeable.RequiredResources);
        }
        else
        {
            Debug.Log("not buildable?");
            
        }
    }



    void OpenBuildMenu()
    {
        buildMenu=false; // change this set buildMenu to false AFTER building is selected
        // only when build menu is implemented
    }
    
    void CloseBuildMenu()
    {
        buildMenu = true;
        isBuilding = false;
        // selectedBuilding = null;
    }
    
}
