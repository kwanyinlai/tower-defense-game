using UnityEngine;

public class BuildMode : MonoBehaviour
{
    public GameObject barracksPrefab;
    public int offset = -6;
    public bool building = false;
    [SerializeField] private GameObject selectedBuilding;
    private GridSystem grid; 
    private bool buildMenu;




    void Start(){
        grid = GameObject.Find("grid-manager").GetComponent<GridSystem>();
        buildMenu = true; 
    }
    
    void Update()
    {

        if(!gameObject.GetComponent<TroopManagment>().inProgress){
            if (Input.GetKeyDown(KeyCode.B))
            {
                building = !building;
                
                
            }

            if(building){
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
            }
        }
       
    }

    public void PlaceBuilding(GameObject selectedBuilding) // call the outline
    {
        
        Vector3 placementPosition = transform.position + transform.forward * offset;

        Placeable temp = selectedBuilding.GetComponent<Placeable>();
        if (temp.IsBuildable(placementPosition))
        {
            Instantiate(temp.prefab, grid.GridToCoordinates(grid.CoordinatesToGrid(placementPosition)), grid.SnapRotation(transform.rotation));
            grid.OccupyArea(placementPosition,temp.size);
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
        building = false;
        // selectedBuilding = null;
    }
    
}
