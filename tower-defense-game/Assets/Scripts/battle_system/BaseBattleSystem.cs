
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BaseBattleSystem : CombatSystem
{

    void Start()
    {
        currentHealth = maxHealth;
        tagList = new HashSet<string>(viewableTagList);
    }

    void Update()
    {
        if(currentHealth <= 0)
        {
            SceneManager.LoadScene("GameOver");
        }
    }

    protected override void Die()
    {
        GameObject.Find("grid-manager").GetComponent<GridSystem>().StarterTerritoryNotAssigned();
        Destroy(gameObject);

    }
}
