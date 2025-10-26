using UnityEngine;
using System.Collections.Generic;

public class TroopCombatSystem : CombatSystem
{
    [Header("Combat Attributes")]
    [SerializeField] protected float attackRange = 5.0f;
    public float AttackRange { get { return attackRange; } set { attackRange = value; } }
    [SerializeField] protected int dmg = 10;
    public int Damage
    {
        get { return dmg; }
        set { dmg = value; }
    }
    [SerializeField] protected float aggroRange = 10.0f;
    public float AggroRange { get { return attackRange; } set { attackRange = value; } } // range which troop engages enemy
    [SerializeField] protected Transform enemyTarget;
    public Transform EnemyTarget { get { return enemyTarget; } set { enemyTarget = value; } }

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
        tagList = new HashSet<string>(viewableTagList);
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
