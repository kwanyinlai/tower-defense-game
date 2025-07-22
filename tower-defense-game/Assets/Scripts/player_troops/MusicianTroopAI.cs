using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;

public class MusicianTroopAI : TroopAI
{

    public float attackBuffStrengthDecimal = 1.25f;
    public float attackCooldown = 2f;
    protected List<GameObject> allyList = null;

    public override void Attack(Transform bulletTarget)
    {
        transform.LookAt(bulletTarget);
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w); // essentially only allows the y axis to move

        Collider collider = this.GetComponent<Collider>();
        atkTimer = atkCooldown;
    }

    public void BuffAttack(List<GameObject> allyList)
    {
        // CombatSystem targetCombat; // commenting out for clarity, uncomment if needed
        foreach(var targetAlly in allyList)
        {
            targetAlly.GetComponent<CombatSystem>().ApplyEffect("attackBuff", attackBuffStrengthDecimal, attackCooldown);
        }

        Collider collider = this.GetComponent<Collider>();
        atkTimer = atkCooldown;
    }
    
    public List<GameObject> GetAlliesInRange()
    {        
        List<GameObject> allies = new List<GameObject>();

        // adds allies within range
        foreach (var ally in TroopAI.troops)
        {
            CombatSystem targetCombat = ally.GetComponent<CombatSystem>();
            float distance = Vector3.Distance(transform.position, ally.transform.position);

            if (distance <= aggroRange)
            {
                allies.Add(ally);
            }
        }
        
        return allies.Count == 0 ? null : allies;
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
        
        allyList = GetAlliesInRange();
        
        if (allyList != null && allyList.Count != 0)
        {
            inCombat = true;
            enemyTarget = allyList[0].transform;
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
                BuffAttack(allyList); 
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
        sellResources.Add("Wood", 100);
        sellResources.Add("TestResource2", 100);
    }

}
