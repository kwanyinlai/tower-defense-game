using UnityEngine;
using System.Collections.Generic;

public class SectorManager : MonoBehaviour
{
    public static SectorManager Instance { get; private set; }
    private GridNode[,] grid;
    private Dictionary<GridNode, Sector> nodeSectorMap = new Dictionary<GridNode, Sector>();
    private List<Sector> sectors = new List<Sector>();


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

    void InitializeSector(){
        sectors.Clear();
        nodeSectorMap.Clear();
    }
}
