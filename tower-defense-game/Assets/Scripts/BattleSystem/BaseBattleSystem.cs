
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


// TODO: This entire script needs to be reworked

public class BaseBattleSystem : CombatSystem
{

    private static float health;

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
        health = currentHealth;
        tagList = new HashSet<string>(viewableTagList);
    }

    protected override void Update()
    {
        base.Update();
        health = currentHealth;
    }

    public static int getHealth()
    {
        return (int)health;
    }

    protected override void Die()
    {
        GridManager.Instance.UnassignTerritory();
        if (gameObject.tag == "Target")
        {
            SceneManager.LoadScene("GameOver");
        }
        Destroy(gameObject);

    }
}
