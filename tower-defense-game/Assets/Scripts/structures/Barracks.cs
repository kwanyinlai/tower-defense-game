using UnityEngine;

public class Barracks : MonoBehaviour
{
    public GameObject troopPrefab;
    public float spawnInterval = 3f;
    public int maxTroops = 10;
    private float timer = 0f;
    private int currentTroops = 0;

    private void Update()
    {
        if (currentTroops < maxTroops)
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                SpawnTroop();
                timer = 0f; 
            }
        }
    }

    private void SpawnTroop()
    { 
        GameObject troop = Instantiate(troopPrefab, transform.position, Quaternion.identity);
        troop.GetComponent<PlayerBattleSystem>().setBarracks(this);
        currentTroops++; 
    }

    public void decrementTroops(){
        currentTroops -= 1;
    }
}
