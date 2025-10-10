using UnityEngine;
using System;
using System.Linq;

public class SnapToGrid : MonoBehaviour
{
    [SerializeField] private GameObject terrain;
    private Placeable placeable;
    private BuildMode buildMode;
    [SerializeField] private GameObject commandingPlayer;


    private LayerMask floorLayer = 1 << 8;


    private Vector3[] TILEOFFSET = {new Vector3(0f,0f,4f), new Vector3(0f,0f,-4f), 
                                  new Vector3(4f,0f,0f), new Vector3(-4f,0f,0f)};

    private int offset;

    void Start(){
        //buildMode = commandingPlayer.GetComponent<BuildMode>();
        //Just a failsafe for when object is insantiated and needs to initialize variables
        //if (commandingPlayer != null)
        //{
        //    InitSnapToGrid(terrain, commandingPlayer);
        //}


    }

    public void InitSnapToGrid(GameObject terrain, GameObject commandingPlayer)
    {
        this.terrain = terrain;
        this.commandingPlayer = commandingPlayer;

        placeable = gameObject.GetComponent<Placeable>();
        buildMode = commandingPlayer.GetComponent<BuildMode>();
        offset = placeable.offset;
    }

    void Update()
    {
        Vector3? mouseNullCheck = GetMousePosOnPlane();
        if (mouseNullCheck == null)
        {
            // show red outline
            return;
        }
        Vector3 mousePos = (Vector3)mouseNullCheck;
        transform.position = GridManager.GridToCoordinates(GridManager.CoordinatesToGrid(
            mousePos));
        // for this model, the centre of the model is off for some reason so it looks wrong. i'm pretty sure
        // it works correctly though, if we replace it with another model for exmaple


        // rotation needs to be figured out
        // prevent from placing on player's position

        if (buildMode.IsPlacingBuilding && !buildMode.buildMenuOpen && placeable.IsBuildable(gameObject.transform.position))
        {
            transform.localScale = new Vector3(2f, 2f, 2f);
        }
        else
        {
            Vector3? closestGrid = NearestGrid();
            if (closestGrid != null)
            {
                transform.position = (Vector3)closestGrid;
                return;
            }

            transform.localScale = new Vector3(0f, 0f, 0f);
        }
        // TODO: waypoint outline missing


    }

    Vector3? NearestGrid(){
 
        float[] distances = new float[4];

        Vector3 mousePos = (Vector3)GetMousePosOnPlane();
        if (mousePos == null) {
            return null;
        }
        for (int i = 0 ; i<4 ; i++){
            if(placeable.IsBuildable( transform.position + TILEOFFSET[i] ) ){

                distances[i] = Vector3.Distance(mousePos, transform.position + TILEOFFSET[i]);
            }
            else{
                distances[i] = float.MaxValue;

            }
           

        }
        float minDistance = distances.OrderBy(n => n).First();
        if(minDistance == float.MaxValue){
            return null;
        }
        int ind = Array.IndexOf(distances, minDistance);
        
        if(ind==-1){
            return null;
        }
        return transform.position + TILEOFFSET[ind];

    }

    Vector3? GetMousePosOnPlane()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorLayer) && hit.collider.gameObject == terrain)
        {
           return hit.point; 
            
        }
        return null;
    }
}
