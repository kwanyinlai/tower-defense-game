using UnityEngine;

public class SnapToGrid : MonoBehaviour
{
    [SerializeField] private Transform commandingPlayer;
    private Placeable placeable;
    private BuildMode buildMode;

    private int offset = -7;

    void Start(){
        placeable = GetComponent<Placeable>();
        buildMode = commandingPlayer.GetComponent<BuildMode>();
    }
    
    void Update()
    {
        transform.position = GridSystem.GridToCoordinates(GridSystem.CoordinatesToGrid(commandingPlayer.position + commandingPlayer.forward * offset));
        transform.rotation = GridSystem.SnapRotation(commandingPlayer.rotation);
        
        if(buildMode.building && !buildMode.buildMenu && placeable.IsBuildable(commandingPlayer.position + commandingPlayer.forward * offset)){
            transform.localScale=new Vector3(2f,2f,2f);
        }
        else{
            transform.localScale=new Vector3(0f,0f,0f);
        }
    }
}
