using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TroopTargetSelector : MonoBehaviour
{
    private float aggroRange;
    private TroopFaction faction;

    private Transform currentTarget;
    private float targetRefreshTimer;
    private float aggroRangeBufferMultiplier = 1.2f;
    private const float TARGET_REFRESH_INTERVAL = 0.5f; 

    [Header("Target Type")]
    [SerializeField] private bool targetAllies;

    public void Initialize(float range, TroopFaction faction)
    {
        this.aggroRange = range;
        this.faction = faction;
    }


    public Transform GetBestTarget(Vector3 position)
    {
        targetRefreshTimer -= Time.deltaTime;

        if (targetRefreshTimer <= 0 || currentTarget == null)
        {
            currentTarget = SelectBestTargetCandidate(position);
            targetRefreshTimer = TARGET_REFRESH_INTERVAL;
        }

        if (currentTarget != null)
        {
            if (!IsValidTarget(currentTarget, position))
            {
                currentTarget = null;
            }
        }

        return currentTarget;
    }

    private Transform SelectBestTargetCandidate(Vector3 position)
    {
        // List<Transform> enemies = FactionManager.Instance.GetEnemiesOf(faction);

        // if (enemies == null || enemies.Count == 0)
        // {
        //     return null;
        // }

        // List<TargetCandidate> candidates = new List<TargetCandidate>();

        // foreach (TroopAI enemy in enemies)
        // {
        //     if (enemy == null || enemy.gameObject == null) continue;

        //     float distance = Vector3.Distance(position, enemy.transform.position);

        //     if (distance <= aggroRange)
        //     {
        //         candidates.Add(new TargetCandidate
        //         {
        //             transform = enemy.transform,
        //             distance = distance,
        //             controller = enemy
        //         });
        //     }
        // }

        // if (candidates.Count == 0)
        // {
        //     return null;
        // }

        // return ChoosePriorityTarget(candidates);
        return null;
        // TODO:

    }

    private Transform ChoosePriorityTarget(List<TargetCandidate> candidates)
    {
        // assumes list is at least size 1
        // sort by priority; curr distance, perhaps inject sorting from parameter
        candidates.Sort((a, b) => a.distance.CompareTo(b.distance));


        return candidates[0].transform;
    }

    // private List<Transform> SelectKPriorityTargets(List<TargetCandidate> candidates, int k = 1)
    // {
    //     // assumes list is at least size k
    //     // sort by priority; curr distance, perhaps inject sorting from parameter
    //     candidates.Sort((a, b) => a.distance.CompareTo(b.distance));


    //     return candidates.Slice(0, k).transform;
    // }

    private bool IsValidTarget(Transform target, Vector3 position)
    {
        if (target == null) return false;

        TroopAI controller = target.GetComponent<TroopAI>();
        if (controller == null || controller.GetCurrentState() == TroopState.Dead)
        {
            return false;
        }

        float distance = Vector3.Distance(position, target.position);
        if (distance > aggroRange * aggroRangeBufferMultiplier) 
        { // give buffer to prevent rapid target switching
            return false;
        }

        return true;
    }

    #region Public Interface

    public void ForceTarget(Transform target)
    {
        currentTarget = target;
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }

    #endregion

    private struct TargetCandidate
    {
        public Transform transform;
        public float distance;
        public TroopAI controller;
    }
}