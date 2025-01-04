using UnityEngine;

public class SnapToGrid : MonoBehaviour
{
    [SerializeField] private Transform commandingPlayer;
    private Placeable placeable;
    private BuildMode buildMode;

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

            transform.localScale=new Vector3(0f,0f,0f);
            Debug.Log(transform.localScale);
        }


    }
}
