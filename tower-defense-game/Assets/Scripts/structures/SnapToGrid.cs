using UnityEngine;

public class SnapToGrid : MonoBehaviour
{
    [SerializeField] private Transform commandingPlayer;
    private GridSystem grid;
    private Placeable placeable;
    private BuildMode buildMode;

    private int offset = -7;

    void Start(){
        grid = GameObject.Find("grid-manager").GetComponent<GridSystem>();
        placeable = GetComponent<Placeable>();
        buildMode = commandingPlayer.GetComponent<BuildMode>();
    }
    
    void Update()
    {
        transform.position = grid.GridToCoordinates(grid.CoordinatesToGrid(commandingPlayer.position + commandingPlayer.forward * offset));
        transform.rotation = grid.SnapRotation(commandingPlayer.rotation);
        
        if(buildMode.building && !buildMode.buildMenu && placeable.IsBuildable(commandingPlayer.position + commandingPlayer.forward * offset)){
            transform.localScale=new Vector3(2f,2f,2f);
        }
        else{
            transform.localScale=new Vector3(0f,0f,0f);
        }
    }
}
