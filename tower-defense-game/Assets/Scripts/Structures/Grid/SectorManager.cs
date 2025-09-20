using UnityEngine;
using System.Collections.Generic;

public class SectorManager : MonoBehaviour
{
    public static SectorManager Instance { get; private set; }
    private GridNode[,] grid;
    private Dictionary<GridNode, GridSector> nodeSectorMap = new Dictionary<GridNode, GridSector>();
    private List<GridSector> sectors = new List<GridSector>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start(){
        grid = GridManager.Instance.GetGrid();
    }

    void InitializeSector()
    {
        sectors.Clear();
        nodeSectorMap.Clear();
        for (int x = 0; x < grid.size; x++)
        {
            for (int y = 0; y < grid.size; y++)
            {
                pass;
            }
        }
    }
}
