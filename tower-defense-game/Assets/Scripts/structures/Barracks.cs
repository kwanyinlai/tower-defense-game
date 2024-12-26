using Unity.VisualScripting;
using UnityEngine;

public class Barracks : MonoBehaviour
{
    public GameObject troopPrefab;
    public float spawnInterval = 3f;
    public int maxTroops = 10;
    private float timer = 0f;
    private int currentTroops = 0;
    private Transform troopEmptyObject;

    private void Start(){
        troopEmptyObject = GameObject.Find("troops").transform;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if(timer >= spawnInterval)
        {
            if(currentTroops < maxTroops)
            {
                SpawnTroop();
                timer = 0f;
            }
        }
    }

    private void SpawnTroop()
    {
        int direction = (int) Mathf.Round(transform.eulerAngles.y / 90); //0 for north (up), 1 for east (right) 2 for south (down), 3 for west (left).
        Vector3 spawnPos = transform.position;
        switch (direction)
        {
            case 0:
                spawnPos.z -= 2.5f;
                break;
            case 1:
                spawnPos.x += 2.5f;
                break;
            case 2:
                spawnPos.z += 2.5f;
                break;
            case 3:
                spawnPos.x -= 2.5f;
                break;
            default:
                Debug.Log("Error With Troop Spawn Direction");
                break;
        }
        GameObject troop = Instantiate(troopPrefab, spawnPos, Quaternion.identity, troopEmptyObject);
        troop.GetComponent<PlayerBattleSystem>().setBarracks(this);
        currentTroops++; 
    }

    public void DecrementTroops(){
        currentTroops -= 1;
    }
}
