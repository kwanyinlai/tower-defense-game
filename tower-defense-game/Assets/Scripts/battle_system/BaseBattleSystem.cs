
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BaseBattleSystem : CombatSystem
{

    private static float health;
    void Start()
    {
        currentHealth = maxHealth;
        health = currentHealth;
        tagList = new HashSet<string>(viewableTagList);
    }

    void Update()
    {
        health = currentHealth;
    }

    public static int getHealth()
    {
        return (int)health;
    }

    protected override void Die()
    {
        GameObject.Find("grid-manager").GetComponent<GridSystem>().StarterTerritoryNotAssigned();
        if (this.gameObject.tag == "Target")
        {
            SceneManager.LoadScene("GameOver");
        }
        Destroy(gameObject);

    }
}
