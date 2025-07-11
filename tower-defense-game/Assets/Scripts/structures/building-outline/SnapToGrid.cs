using UnityEngine;
using System;
using System.Linq;

public class SnapToGrid : MonoBehaviour
{
    [SerializeField] private GameObject terrain;
    private Placeable placeable;
    private BuildMode buildMode;
    [SerializeField] private GameObject commandingPlayer;
    [SerializeField] private GameObject mainCamera;



    private Vector3[] TILEOFFSET = {new Vector3(0f,0f,4f), new Vector3(0f,0f,-4f), 
                                  new Vector3(4f,0f,0f), new Vector3(-4f,0f,0f)};

    private int offset;

    void Start(){
        placeable = gameObject.GetComponent<Placeable>();
        buildMode = commandingPlayer.GetComponent<BuildMode>();
        offset = gameObject.GetComponent<Placeable>().offset;
        Debug.Log("offset is " + offset);
    }
    
    void Update()
    {
        Vector3 mousePos = GetMousePosOnPlane();
        transform.position = GridSystem.GridToCoordinates(GridSystem.CoordinatesToGrid(
            mousePos));
        // for this model, the centre of the model is off for some reason so it looks wrong. i'm pretty sure
        // it works correctly though, if we replace it with another model for exmaple


        // rotation needs to be figured out
        // buggy position tracking with Vector.zero
        // prevent from placing on player's position

 
        if(buildMode.isBuilding && !buildMode.buildMenuOpen && placeable.IsBuildable(gameObject.transform.position))
        {

            transform.localScale=new Vector3(2f,2f,2f);
        }
        else{
            if(!placeable.IsBuildable(gameObject.transform.position)){
                
                if ( NearestGrid() !=null){
                    transform.position = (Vector3) NearestGrid();
                    return;
                }
            }

            transform.localScale=new Vector3(0f,0f,0f);
        }


    }

    Vector3? NearestGrid(){
 
        float[] distances = new float[4];
        for(int i = 0 ; i<4 ; i++){
            if(placeable.IsBuildable( transform.position + TILEOFFSET[i] ) ){
                distances[i] = Vector3.Distance(GetMousePosOnPlane(), transform.position + TILEOFFSET[i]);
            }
            else{
                distances[i] = float.MaxValue; 
            }
           

        } 
        float minDistance = distances.OrderBy(n => n) 
                                    .Distinct()
                                    .FirstOrDefault();
        if(minDistance == float.MaxValue){
            return null;
        }
        int ind = Array.IndexOf(distances, minDistance);
        
        if(ind==-1){
            return null;
        }
        return transform.position + TILEOFFSET[ind];

    }

    Vector3 GetMousePosOnPlane()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == terrain)
        {
           return hit.point; 
            
        }
        return Vector3.zero;
    }
}
