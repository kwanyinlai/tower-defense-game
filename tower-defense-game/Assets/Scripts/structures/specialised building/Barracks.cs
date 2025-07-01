using Unity.VisualScripting;
using UnityEngine;

public class Barracks : Building
{
    public GameObject troopPrefab;
    public float spawnInterval = 3f;
    public float maxTroops = 10f;
    private float timer = 0f;
    private int currentTroops = 0;
    private Transform troopEmptyObject;

    
    private void Start(){
        base.Start();
        troopEmptyObject = GameObject.Find("troops").transform;
    }

    protected override void IntializeSellResources()
    {
        //TODO: Remove and replace with code to actually add the correct resources based on building
        sellResources.Add("TestResource1", 100);
        sellResources.Add("TestResource2", 100);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval && currentTroops < maxTroops)
        {
            SpawnTroop();
            timer = 0f;
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
        troop.GetComponent<TroopCombatSystem>().setBarracks(this);
        currentTroops++; 
    }

    public void DecrementTroops(){
        currentTroops--;
    }
}
