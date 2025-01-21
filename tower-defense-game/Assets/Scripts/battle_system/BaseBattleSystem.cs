
using UnityEngine;

public class BaseBattleSystem : BattleSystem
{

    void Start()
    {
        currentHealth = maxHealth;
    }
    protected override void Die()
    {
        GameObject.Find("grid-manager").GetComponent<GridSystem>().StarterTerritoryNotAssigned();
        Destroy(gameObject);

    }
}
