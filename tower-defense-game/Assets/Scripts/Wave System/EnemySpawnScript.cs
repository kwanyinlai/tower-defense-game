using UnityEngine;

public class EnemySpawnScript : MonoBehaviour
{
    private Vector3 spawnPosition = new Vector3(40f, 0.5f, 40f);
    private float spawnDelay;
    private float spawnTimer;
    public GameObject enemy;

    void Start()
    {
        spawnDelay = 5f;
        spawnTimer = 0f;
    }

    void Update()
    {
        
    }

    public void SpawnEnemies(int wave)
    {
        int spawned = 0;
        while (spawned < wave)
        {
            spawnTimer += Time.deltaTime;
            spawnDelay = 5f / wave;
            if(spawnTimer > spawnDelay)
            {
                Instantiate(enemy, spawnPosition, Quaternion.identity);
                spawnTimer = 0f;
                spawned++;
            }
        }
    }
}
