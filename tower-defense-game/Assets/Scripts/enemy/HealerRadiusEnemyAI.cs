using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;

public class HealerRadiusEnemyAI : EnemyAI
{

    public float healStrength = 10;
    public float healCooldown = 2;
    protected List<GameObject> allyList = null;

    public override void Attack(Transform bulletTarget)
    {
        transform.LookAt(bulletTarget);
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w); // essentially only allows the y axis to move


        Collider collider = this.GetComponent<Collider>();
        atkTimer = atkCooldown;
    }

    public void Heal(List<GameObject> allyList)
    {
        CombatSystem targetCombat;
        Debug.Log("Checking!");
        foreach(var targetAlly in allyList)
        {
            targetCombat = targetAlly.GetComponent<CombatSystem>();
            float previousHealStrength = targetCombat.GetEffectStrength("heal");
            if(previousHealStrength < healStrength)
            {
                targetCombat.ApplyEffect("heal", healStrength, healCooldown);
                Debug.Log("Healed");
                targetCombat.AddHealth(healStrength - previousHealStrength);
            }
        }

        Collider collider = this.GetComponent<Collider>();
        atkTimer = atkCooldown;
    }
    
    public List<GameObject> GetAlliesInRange()
    {        
        List<GameObject> allies = new List<GameObject>();

        // adds allies within range
        foreach (var ally in EnemyAI.enemies)
        {
            CombatSystem targetCombat = ally.GetComponent<CombatSystem>();
            float distance = Vector3.Distance(transform.position, ally.transform.position);

            if (distance <= aggroRange && targetCombat.currentHealth != targetCombat.maxHealth)
            {
                allies.Add(ally);
            }
        }
        
        return allies.Count == 0 ? null : allies;
    }

    void Update()
    {

        if (baseTarget == null)
        {
            Debug.Log("No base found!");
            baseTarget = GameObject.Find("base-manager").GetComponent<BaseManager>().GetBase();
        }
        else
        {
            targetStats = baseTarget.GetComponent<CombatSystem>();
        }
        
        allyList = GetAlliesInRange();

        // sets troop target just in case something else depends on it
        if(allyList == null)
        {
            troopTarget = null;
        }
        else
        {
            troopTarget = allyList[0].transform;
        }

        barracksTarget = GetClosestBarracksInRange();

        if (troopTarget != null)
        {
            targetStats = troopTarget.GetComponent<CombatSystem>();
            float distance = Vector3.Distance(transform.position, troopTarget.position);
            if (distance <= range)
            {
                StopMoving();
                if (atkTimer <= 0f)
                {
                    Heal(allyList);
                }
            }
            else
            {
                MoveTowardsTarget(troopTarget);
            }
        }
        else if (baseTarget != null)
        {

            float distance = Vector3.Distance(agent.transform.position, baseTarget.transform.position);
            if (distance <= range)
            {
                StopMoving();
                if (agent.velocity.magnitude <= 0.1f)
                {
                    if (atkTimer <= 0f)
                    {
                        Attack(baseTarget.transform);
                    }
                }

            }
            else
            {
                MoveTowardsTarget(baseTarget.transform);
            }
        }
        else
        {
            StopMoving();
        }
        if (atkTimer > 0f) {
            atkTimer -= Time.deltaTime;
        } else
        {
            atkTimer = atkCooldown;
        }
    } 

}
