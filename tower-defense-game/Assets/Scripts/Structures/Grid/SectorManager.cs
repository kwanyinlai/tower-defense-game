using UnityEngine;
using System.Collections.Generic;

public class SectorManager : MonoBehaviour
{
    public static SectorManager Instance { get; private set; }
    private GridManager.GridNode[,] grid;
    private Dictionary<GridManager.GridNode, GridSector> nodeSectorMap = new Dictionary<GridManager.GridNode, GridSector>();
    private List<GridSector> sectors = new List<GridSector>();
    
    // store sectors as quadtree?


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

    void Start()
    {
        grid = GridManager.Instance.GetGrid();
    }

    void InitializeSector()
    {
        sectors.Clear();
        nodeSectorMap.Clear();
        for (int x = 0; x < grid.Length; x++)
        {
            for (int y = 0; y < grid.Length; y++)
            {
                return;
            }
        }
    }

    void MergeSector()
    {

    }

    void SplitSector()
    {
        
    }
}
