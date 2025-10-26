using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;
public abstract class EnemyAI : TroopAI
{
    protected static List<GameObject> allEnemyTroops = new List<GameObject>();
    public static List<GameObject> AllEnemyTroops { get; }
    protected Transform baseTarget;
    protected Transform troopTarget;
    protected Transform barracksTarget;


    protected virtual void Start()
    {
        base.Start();
    }

    protected override void AddEntityToAliveList()
    {
        allEnemyTroops.Add(gameObject);
    }

    public override void RemoveEntityFromAliveList()
    {
        allEnemyTroops.Remove(gameObject);
    }

    public override List<GameObject> GetEntityAliveList()
    {
        return allEnemyTroops;
    }


    protected virtual void Update()
    {
        base.Update();
    }

    protected Transform GetPlayerBaseTarget()
    {
        if (baseTarget == null)
        {
            return BaseManager.Instance.GetBase().transform;
        }
        else
        {
            return null;
        }
        
    }
    protected override void FindAndSetAllTargets()
    {
        baseTarget = GetPlayerBaseTarget();
        troopTarget = GetClosestEnemyInRange(); // TODO: Use ITroopBehaviour and isinstance of ISupport or IAttack to select target
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
        if (navMeshAgent.isStopped && troopTarget != null)
        {
            return troopTarget;
        }
        GameObject closestEnemy = null;
        float closestDistance = aggroRange;
        if (PlayerTroopAI.AllPlayerTroops.Count == 0)
        {
            return null;
        }
        foreach (var enemy in PlayerTroopAI.AllPlayerTroops)
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

    public static List<GameObject> GetAllyEntitiesAliveList()
    {
        return allEnemyTroops;
    }
    public static List<GameObject> GetEnemyEntitiesAliveList()
    {
        return PlayerTroopAI.GetAllyEntitiesAliveList();
    }
}
