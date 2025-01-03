using UnityEngine;

public class BuildMode : MonoBehaviour
{


    public bool isBuilding = false;
    [SerializeField] private GameObject selectedBuilding;
    public bool buildMenu;

    [Header("Building Prefabs")]
    public GameObject barracksPrefab;


    void Start(){
        buildMenu = true; 
        isBuilding=false;
    }
    
    void Update()
    {

        if(!gameObject.GetComponent<TroopManagment>().inProgress){
            

            if(isBuilding){
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CloseBuildMenu();
                }
                if(buildMenu){
                    OpenBuildMenu();
                }
                else{
                    ShowBuildingOutline();
                    
                    if (Input.GetKeyDown(KeyCode.Return)){
                        
                        PlaceBuilding(selectedBuilding);
                        CloseBuildMenu();
                    }
                }



            }
            else{
                HideBuildingOutline();
                if (Input.GetKeyDown(KeyCode.B))
                {
                    isBuilding = true;
                }

            }
        }
       
    }

    public void PlaceBuilding(GameObject selectedBuilding) // call the outline
    {
        
        Placeable placeable = selectedBuilding.GetComponent<Placeable>();
        int offset = placeable.offset;
        Vector3 placementPosition = transform.position + transform.forward * offset;

        if (placeable.IsBuildable(placementPosition))
        {
            GameObject building = Instantiate(placeable.prefab, 
                            GridSystem.GridToCoordinates(GridSystem.CoordinatesToGrid(placementPosition)), 
                            GridSystem.SnapRotation(transform.rotation));
            GridSystem.OccupyArea(placementPosition, placeable.size, 
                            building.GetComponent<Building>().range);
        }
        else
        {
            Debug.Log("not buildable?");
            
        }
    }

    void ShowBuildingOutline(){
        
      
        selectedBuilding.transform.localScale= new Vector3(2f,2f,2f);
            
        
    }

    void HideBuildingOutline(){
        selectedBuilding.transform.localScale= new Vector3(0f,0f,0f);
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
