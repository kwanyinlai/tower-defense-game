using UnityEngine;

public class SnapToGrid : MonoBehaviour
{
    [SerializeField] private Transform commandingPlayer;
    private GridSystem grid;

    void Start(){
        grid = GameObject.Find("grid-manager").GetComponent<GridSystem>();
    }
    
    void Update()
    {
        transform.position = grid.GridToCoordinates(grid.CoordinatesToGrid(commandingPlayer.position+ commandingPlayer.forward * -6));
        transform.rotation = grid.SnapRotation(commandingPlayer.rotation);
    }
}
