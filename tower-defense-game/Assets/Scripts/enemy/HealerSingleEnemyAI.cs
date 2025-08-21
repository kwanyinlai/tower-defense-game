using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;

public class HealerSingleEnemyAI : SupportEnemyAI
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
        float previousHealStrength = targetCombat.GetEffectStrength("heal");
        targetCombat.ApplyEffect("heal", healStrength, healCooldown);
        targetCombat.AddHealth(healStrength - previousHealStrength);

        Collider collider = this.GetComponent<Collider>();
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
        // Get Targets
        troopTarget = GetBestAllyInRange();
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
    }
}
