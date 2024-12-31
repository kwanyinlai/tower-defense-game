using UnityEngine;

public class SnapToGrid : MonoBehaviour
{
    [SerializeField] private Transform commandingPlayer;
    private GridSystem grid;
    private Placeable placeable;

    void Start(){
        grid = GameObject.Find("grid-manager").GetComponent<GridSystem>();
        placeable = GetComponent<Placeable>();
    }
    
    void Update()
    {
        transform.position = grid.GridToCoordinates(grid.CoordinatesToGrid(commandingPlayer.position + commandingPlayer.forward * -6));
        transform.rotation = grid.SnapRotation(commandingPlayer.rotation);
        
        if(placeable.IsBuildable(commandingPlayer.position + commandingPlayer.forward * -6)){
            transform.localScale=new Vector3(2f,2f,2f);
        }
        else{
            transform.localScale=new Vector3(0f,0f,0f);
            Debug.Log("not buildable");
        }
    }
}
