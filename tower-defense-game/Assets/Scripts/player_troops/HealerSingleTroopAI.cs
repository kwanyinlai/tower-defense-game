using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;

public class HealerSingleTroopAI : TroopAI
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
    
    public Transform GetBestAllyInRange()
    {
        if (TroopAI.troops.Count == 0)
        {
            return null;
        }

        List<GameObject> potentialAllies = new List<GameObject>();

        // adds allies within range
        foreach (var ally in TroopAI.troops)
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
                    maxHealth = currMaxHealth;
                }
            }
        }

        if (bestAlly == null) { return null; }
        return bestAlly.transform;
    }


    protected override void Update()
    {
        if (atkTimer > 0f) { atkTimer -= Time.deltaTime; }
        if(underSelection){
            ;
        }
        else{
            HideCircle();
        }
        Transform copy = GetBestAllyInRange();
        if (copy != null)
        {
            inCombat = true;
            enemyTarget = copy;
        }
        else
        {   
            inCombat = false;  
            agent.isStopped=false; 
        }
       
        if (inCombat)
        {
            agent.speed = maxSpeed * (1 - combatSystem.GetEffectStrength("slow"));
            ApplyEffectToAlly();
        } 
        else {        
            agent.speed = maxSpeed * (1 - combatSystem.GetEffectStrength("slow"));
            if(underSelection){

                ;
            }
            else{
                if(waypoint!=null){
                    GoToWaypoint();
                    HideCircle();
                    
                }
                else{
                    ;
                }

            }   
            
        }
        
    }

    protected void ApplyEffectToAlly(){
        
        targetStats = enemyTarget.GetComponent<CombatSystem>(); 
        float distance = Vector3.Distance(transform.position, enemyTarget.position);
        if (distance <= attackRange /*&& agent.velocity.magnitude <= 0.1f*/) {
            agent.isStopped = true;
            if (atkTimer <= 0f && agent.velocity.magnitude <= 0.05)  { 
                Heal(enemyTarget); 
            }
        }
        else{ 
            agent.isStopped = false;
            agent.SetDestination(enemyTarget.position); 
        }
    }

    protected override void IntializeSellResources()
    {
        //TODO: Remove and replace with code to actually add the correct resources based on building
        sellResources.Add("TestResource1", 100);
        sellResources.Add("TestResource2", 100);
    }
}
