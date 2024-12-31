using UnityEngine;

public class BuildMode : MonoBehaviour
{
    public GameObject barracksPrefab;
    public int offset = -6;
    public bool building = false;
    [SerializeField] private GameObject barracksOutline;


    // TODO: LOCK TO GRID
    
    void Update()
    {
        if(!gameObject.GetComponent<TroopManagment>().inProgress){
            if (Input.GetKeyDown(KeyCode.B))
            {
                building = !building;
                
                
            }

            if(building){
                ShowBuildingOutline();
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
        Debug.Log("building");
        Vector3 placementPosition = transform.position + transform.forward * offset;

        
        Instantiate(barracksPrefab, placementPosition, transform.rotation);
    }

    void ShowBuildingOutline(){
        
      
        barracksOutline.transform.localScale= new Vector3(2f,2f,2f);
            
        
    }

    void HideBuildingOutline(){
        barracksOutline.transform.localScale= new Vector3(0f,0f,0f);
    }
    
}
