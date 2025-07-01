using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;

public class HealerEnemyAI : EnemyAI
{

    public float healStrength = 10;
    public float healCooldown = 2;

    public override void Attack(Transform bulletTarget)
    {

        transform.LookAt(bulletTarget);
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w); // essentially only allows the y axis to move


        Collider collider = this.GetComponent<Collider>();
        atkTimer = atkCooldown;
    }

    public void Heal(Transform bulletTarget)
    {
        transform.LookAt(bulletTarget);
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w); // essentially only allows the y axis to move

        CombatSystem targetCombat = bulletTarget.GetComponent<CombatSystem>();
        targetCombat.ApplyEffect("heal", healStrength, healCooldown);
        targetCombat.AddHealth(healStrength);

        Collider collider = this.GetComponent<Collider>();
        atkTimer = atkCooldown;
    }
    
    public Transform GetBestAllyInRange()
    {
        if (EnemyAI.enemies.Count == 0)
        {
            return null;
        }

        
        List<GameObject> potentialAllies = new List<GameObject>();

        // adds allies within range
        foreach (var ally in EnemyAI.enemies)
        {
            float distance = Vector3.Distance(transform.position, ally.transform.position);

            if (distance <= aggroRange)
            {
                potentialAllies.Add(ally);
            }
        }

        GameObject bestAlly = null;

         // remove from potential if ally at 100% or is on cooldown
        for(int i = potentialAllies.Count - 1; i >= 0; i--)
        {
            float currHealth = potentialAllies[i].GetComponent<CombatSystem>().currentHealth;
            float currMaxHealth = potentialAllies[i].GetComponent<CombatSystem>().maxHealth;

            if (currMaxHealth == currHealth || potentialAllies[i].GetComponent<CombatSystem>().GetEffectStrength("heal") != 0)
            {
                potentialAllies.RemoveAt(i);
            }
        }


        if(potentialAllies.Count > 0)
        {
            bestAlly = potentialAllies[0];
            float lowestHealth = bestAlly.GetComponent<CombatSystem>().currentHealth;
            float maxHealth = bestAlly.GetComponent<CombatSystem>().maxHealth;
            
            // determines best ally
            foreach (var ally in potentialAllies)
            {
                float distance = Vector3.Distance(transform.position, ally.transform.position);
                float currHealth = ally.GetComponent<CombatSystem>().currentHealth;
                float currMaxHealth = ally.GetComponent<CombatSystem>().maxHealth;

                if (currHealth <= lowestHealth)
                {
                    bestAlly = ally;
                    lowestHealth = currHealth;
                    maxHealth = maxHealth;
                }
            }
        }

        if (bestAlly == null) { return null; }
        return bestAlly.transform;
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
        
        troopTarget = GetBestAllyInRange();

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
                    Heal(troopTarget);
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
