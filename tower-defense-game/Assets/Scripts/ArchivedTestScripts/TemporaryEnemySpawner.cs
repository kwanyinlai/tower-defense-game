using UnityEngine;

public class TemporaryEnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    void Start()
    {
        Instantiate(enemyPrefab, new Vector3(6.234631f, 4.967472f, -3.947071f), Quaternion.identity);
        Instantiate(enemyPrefab, new Vector3(11.06f, 4.967472f, -3.947071f), Quaternion.identity);
    }


}
