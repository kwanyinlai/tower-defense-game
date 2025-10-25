using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;

public class RadiusHealerBehaviour : ISupportBehaviour
{

    public float healStrength = 10;
    public float healCooldown = 2;
    protected List<GameObject> allyList = null;

    public override void InteractWithTarget(CombatSystem combatSystem)
    {
        ;
    }

    public void Heal(List<GameObject> allyList)
    {
        CombatSystem targetCombat;
        foreach(var targetAlly in allyList)
        {
            targetCombat = targetAlly.GetComponent<CombatSystem>();
            float previousHealStrength = targetCombat.GetEffectStrength("heal");
            if(previousHealStrength < healStrength)
            {
                targetCombat.ApplyEffect("heal", healStrength, healCooldown);
                targetCombat.AddHealth(healStrength - previousHealStrength);
            }
        }

        atkTimer = atkCooldown;
    }
    
    

    protected override void Update()
    {
        if (atkTimer > 0f) {
            atkTimer -= Time.deltaTime;
        } else
        {
            atkTimer = atkCooldown;
        }

        FindDefaultTargets();
        
        FindTargets();

        HandleCombat();

    } 

    protected void FindTargets() {
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
    }

    protected void HandleCombat() {
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
