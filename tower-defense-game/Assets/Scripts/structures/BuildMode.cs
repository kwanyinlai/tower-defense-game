using UnityEngine;

public class BuildMode : MonoBehaviour
{
    public GameObject barracksPrefab;
    public int offset = -6;
    public bool building = false;
    [SerializeField] private GameObject barracksOutline;
    private GridSystem grid; 


    // TODO: LOCK TO GRID

    void Start(){
        grid = GameObject.Find("grid-manager").GetComponent<GridSystem>();
    }
    
    void Update()
    {

        if(!gameObject.GetComponent<TroopManagment>().inProgress){
            if (Input.GetKeyDown(KeyCode.B))
            {
                building = !building;
                
                
            }

            if(building){
                ShowBuildingOutline();
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    building = false;
                }
                if (Input.GetKeyDown(KeyCode.Return)){
                    
                    PlaceBuilding(barracksOutline);
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
        
      
        barracksOutline.transform.localScale= new Vector3(2f,2f,2f);
            
        
    }

    void HideBuildingOutline(){
        barracksOutline.transform.localScale= new Vector3(0f,0f,0f);
    }
    
    
}
