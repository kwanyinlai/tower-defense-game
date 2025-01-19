using UnityEngine;

public class EnemySpawnScript : MonoBehaviour
{
    private Vector3 spawnPosition = new Vector3(40f, 0.5f, 40f);
    private float spawnDelay;
    private float spawnTimer;
    public GameObject enemy;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnDelay = 5f;
        spawnTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void spawnEnemies(int wave)
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
