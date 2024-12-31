using UnityEngine;

public class SnapToGrid : MonoBehaviour
{
    [SerializeField] private Transform commandingPlayer;
    private GridSystem grid;
    private Vector2Int size;

    void Start(){
        grid = GameObject.Find("grid-manager").GetComponent<GridSystem>();
        size = GetComponent<Placeable>().size;
    }
    
    void Update()
    {
        transform.position = grid.GridToCoordinates(grid.CoordinatesToGrid(commandingPlayer.position + commandingPlayer.forward * -6));
        transform.rotation = grid.SnapRotation(commandingPlayer.rotation);
        if(grid.IsTileAreaBuildable(commandingPlayer.position + commandingPlayer.forward * -6, size)){
            transform.localScale=new Vector3(2f,2f,2f);
        }
        else{
            transform.localScale=new Vector3(0f,0f,0f);
        }
    }
}
