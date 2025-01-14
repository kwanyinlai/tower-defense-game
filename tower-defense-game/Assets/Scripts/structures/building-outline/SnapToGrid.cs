using UnityEngine;
using System;
using System.Linq;

public class SnapToGrid : MonoBehaviour
{
    [SerializeField] private Transform commandingPlayer;
    private Placeable placeable;
    private BuildMode buildMode;



    private Vector3[] TILEOFFSET = {new Vector3(0f,0f,4f), new Vector3(0f,0f,-4f), 
                                  new Vector3(4f,0f,0f), new Vector3(-4f,0f,0f)};

    private int offset;

    void Start(){
        placeable = gameObject.GetComponent<Placeable>();
        buildMode = commandingPlayer.GetComponent<BuildMode>();
        offset = gameObject.GetComponent<Placeable>().offset;
    }
    
    void Update()
    {
        transform.position = GridSystem.GridToCoordinates(GridSystem.CoordinatesToGrid(commandingPlayer.position + commandingPlayer.forward * offset));
        transform.rotation = GridSystem.SnapRotation(commandingPlayer.rotation);

 



        if(buildMode.isBuilding && !buildMode.buildMenu && placeable.IsBuildable(gameObject.transform.position))
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
        for(int i = 0; i<4 ; i++){

        }
        

        for(int i = 0 ; i<4 ; i++){
            if(placeable.IsBuildable( transform.position + TILEOFFSET[i] ) ){
                distances[i] = Vector3.Distance(commandingPlayer.transform.position, transform.position + TILEOFFSET[i]);
            }
            else{
                distances[i] = 999; // arbitrarily large;
            }
           

        } 
        float secondMin = distances.OrderBy(n => n) // second min since the first min is the one the player is on
                                    .Distinct()
                                    .Skip(1)
                                    .FirstOrDefault();
        if(secondMin == 999){
            return null;
        }
        int ind = Array.IndexOf(distances, secondMin);
        
        if(ind==-1){
            return null;
        }
        return transform.position + TILEOFFSET[ind];

    }
}
