using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Barracks : Building
{
    public GameObject troopPrefab;
    protected float spawnInterval = 3f;
    protected float maxTroops = 10f;
    protected float timer = 0f;
    protected int currentTroops = 0;
    protected Transform troopEmptyObject;
    public Dictionary<string, int> troopResources = new Dictionary<string, int>();


    new protected void Start(){
        base.Start();
        InitializeTroopResources();
        troopEmptyObject = GameObject.Find("troops").transform;
    }

    protected virtual void InitializeTroopResources()
    {
    }

    protected override void IntializeSellResources()
    {
    }

    protected void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval && currentTroops < maxTroops)
        {
            SpawnTroop();
            timer = 0f;
        }
    }


    protected void SpawnTroop()
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
        if (ResourcePool.EnoughResources(troopResources))
        {
            ResourcePool.DepleteResource(troopResources);
            GameObject troop = Instantiate(troopPrefab, spawnPos, Quaternion.identity, troopEmptyObject);
            troop.GetComponent<TroopCombatSystem>().setBarracks(this);
            currentTroops++;
        }
    }

    public void DecrementTroops(){
        currentTroops--;
    }
}
