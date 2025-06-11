
using UnityEngine;
using System.Collections.Generic;

public class BaseBattleSystem : CombatSystem
{

    void Start()
    {
        currentHealth = maxHealth;
        tagList = new HashSet<string>(viewableTagList);
    }
    protected override void Die()
    {
        GameObject.Find("grid-manager").GetComponent<GridSystem>().StarterTerritoryNotAssigned();
        Destroy(gameObject);

    }
}
