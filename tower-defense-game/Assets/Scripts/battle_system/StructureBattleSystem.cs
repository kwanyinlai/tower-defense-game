using System.Collections.Generic;
using UnityEngine;

public class StructureBattleSystem : BattleSystem
{
    public static List<GameObject> barracks = new List<GameObject>();
    void Start()
    {
        barracks.Add(gameObject);
        currentHealth = maxHealth;
        tagList = new HashSet<string>(viewableTagList);
    }
    protected override void Die()
    {
        barracks.Remove(gameObject);
        base.Die();
    }
}
