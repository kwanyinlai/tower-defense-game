using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class WaveScript : MonoBehaviour
{
    public int waveNum;
    private float gameTimer;
    private float waveTimer;
    public GameObject waveManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        waveNum = 0;
        gameTimer = 0;
        waveTimer = 0;
    }

    // Update is called once per frame
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
            spawner.spawnEnemies(waveNum);
            GridSystem.updateWaveTerritory();
        }
    }
}
