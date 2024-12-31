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
                    
                    PlaceBuilding();
                }
            }
            else{
                HideBuildingOutline();
            }
        }
       
    }

    void PlaceBuilding()
    {
        
        Vector3 placementPosition = transform.position + transform.forward * offset;


        //if (grid.IsBuildable(placementPosition))
        //{
 
            Instantiate(barracksPrefab, grid.GridToCoordinates(grid.CoordinatesToGrid(placementPosition)), grid.SnapRotation(transform.rotation));

        //}
        //else
        //{
        //    Debug.Log("not buildable?");
        //}
    }

    void ShowBuildingOutline(){
        
      
        barracksOutline.transform.localScale= new Vector3(2f,2f,2f);
            
        
    }

    void HideBuildingOutline(){
        barracksOutline.transform.localScale= new Vector3(0f,0f,0f);
    }
    
    
}
