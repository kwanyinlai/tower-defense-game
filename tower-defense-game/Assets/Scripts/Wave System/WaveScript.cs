using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class WaveManager : MonoBehaviour
{
    public int waveNum;
    private float gameTimer;
    private float waveTimer;
    public GameObject waveManager;
    private GridManager gridManager;

    void Start()
    {
        waveNum = 0;
        gameTimer = 0;
        waveTimer = 0;
        gridManager = GridManager.Instance;
    }
    
    public static WaveManager Instance { get; private set; }

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

    void Update()
    {
        gameTimer += Time.deltaTime;
        waveTimer += Time.deltaTime;

        //Default set to 60 seconds per wave, change later for more/less time between waves.

        if (waveTimer >= 60 || (waveNum > 0 && EnemyAI.enemies.Count == 0) || (waveNum == 0 && waveTimer >= 10))
        {
            waveNum++;
            waveTimer = 0;
            EnemySpawnScript spawner = waveManager.GetComponent<EnemySpawnScript>();
            spawner.SpawnEnemies(waveNum);
            gridManager.TerritoryUpdate();
        }
    }
}
