using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;
public abstract class EnemyAI : TroopAI
{
    public static List<GameObject> allEnemyTroops = new List<GameObject>();
    protected Transform baseTarget;
    protected Transform troopTarget;
    protected Transform barracksTarget;


    protected virtual void Start()
    {
        super.Start();
    }

    protected override void AddEntityToAliveList()
    {
        allEnemyTroops.Add(gameObject);
    }

    protected override void RemoveEntityFromAliveList()
    {
        allEnemyTroops.Remove(gameObject);
    }
    


    protected virtual void Update()
    {
        super.Update();
    }

    protected Transform GetPlayerBaseTarget()
    {
        if (baseTarget == null){
            return BaseManager.Instance.GetBase().transform;
        }
        
    }
    protected override void FindAndSetAllTargets()
    {
        baseTarget = GetPlayerBaseTarget();
        troopTarget = GetClosestEnemyInRange();
        barracksTarget = GetClosestBarracksInRange();
        if (troopTarget != null)
        {
            enemyTarget = troopTarget;
        }
        else if (barracksTarget != null)
        {
            enemyTarget = barracksTarget;
        }
        else
        {
            enemyTarget = barracksTarget;
        }
    }


    public Transform GetClosestEnemyInRange()
    {
        if (agent.isStopped && troopTarget != null)
        {
            return troopTarget;
        }
        GameObject closestEnemy = null;
        float closestDistance = aggroRange;
        if (TroopAI.troops.Count == 0)
        {
            return null;
        }
        foreach (var enemy in TroopAI.troops)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance <= aggroRange && distance < closestDistance)
            {
                closestEnemy = enemy;
                closestDistance = distance;
            }
        }
        if (closestEnemy == null) { return null; }
        return closestEnemy.transform;
    }

    
    public Transform GetClosestBarracksInRange()
    {
        if (barracksTarget != null)
        {
            return barracksTarget;
        }
        GameObject closestBarrack = null;
        float closestDistance = aggroRange;
        if (StructureBattleSystem.barracks.Count == 0)
        {
            return null;
        }
        foreach (var barrack in StructureBattleSystem.barracks)
        {
            float distance = Vector3.Distance(transform.position, barrack.transform.position);

            if (distance <= aggroRange && distance < closestDistance)
            {
                closestBarrack = barrack;
                closestDistance = distance;
            }
        }
        if (closestBarrack == null) { return null; }
        return closestBarrack.transform;
    }
}
