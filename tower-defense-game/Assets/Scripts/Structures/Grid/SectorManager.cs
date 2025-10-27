using UnityEngine;
using System.Collections.Generic;

public class SectorManager : MonoBehaviour
{
    public static SectorManager Instance { get; private set; }
    private GridManager.GridNode[,] grid;
    private GridSector[,] sectors = new GridSector[GridManager.gridWidth / GridSector.sectorWidth, GridManager.gridHeight / GridSector.sectorHeight];

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
