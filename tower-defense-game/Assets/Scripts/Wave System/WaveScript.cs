using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class WaveScript : MonoBehaviour
{
    public int waveNum;
    private float gameTimer;
    private float waveTimer;
    public GameObject waveManager;
    private GridSystem gridManager;

    void Start()
    {
        waveNum = 0;
        gameTimer = 0;
        waveTimer = 0;
        gridManager = GameObject.Find("grid-manager").GetComponent<GridSystem>();
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
