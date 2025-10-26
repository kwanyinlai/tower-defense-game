using UnityEngine;
using System.Collections.Generic;

public class TroopCombatSystem : CombatSystem
{
    [Header("Combat Attributes")]
    [SerializeField] protected int atk = 10;
    public int Attack
    {
        get { return atk; }
        set { atk = value; }
    }

    protected float atkCooldown { get; set; } = 1.5f;
    protected float atkTimer { get; set; } = 0f;

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
        tagList = new HashSet<string>(viewableTagList);
    }

    public void DecrementAttackCoooldown(float deltaTime)
    {
        if (atkTimer > 0f) { atkTimer -= deltaTime; }

    }

        
    public bool CanAttack()
    {
        return atkTimer <= 0f;
    }
    
    public void ResetAttackCooldown()
    {
        atkTimer = atkCooldown;
    }

    protected override void Die()
    {
        GetComponent<TroopAI>().RemoveEntityFromAliveList();
        foreach(GameObject player in Player.players){
            List<GameObject> list = player.GetComponent<TroopManagment>().SelectedTroops;
            if (list.Contains(gameObject)){
                list.Remove(gameObject);
                break;
            }
        }

        base.Die();
    }
}
