using UnityEngine;
using System.Collections.Generic;

public class FactionManager : MonoBehaviour
{
    public static FactionManager Instance { get; private set; }

    private Dictionary<TroopFaction, List<TroopAI>> troopsByFaction = new Dictionary<TroopFaction, List<TroopAI>>();

    private void Awake()
    {
        // singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // initialise faction lists
        foreach (TroopFaction faction in System.Enum.GetValues(typeof(TroopFaction)))
        {
            troopsByFaction[faction] = new List<TroopAI>();
        }
    }

    public void RegisterTroop(TroopAI troop, TroopFaction faction)
    {
        if (!troopsByFaction[faction].Contains(troop))
        {
            troopsByFaction[faction].Add(troop);
        }
    }

    public void UnregisterTroop(TroopAI troop, TroopFaction faction)
    {
        troopsByFaction[faction].Remove(troop);
    }

    public List<TroopAI> GetAlliesOf(TroopFaction faction)
    {
        return new List<TroopAI>(troopsByFaction[faction]);
    }

    public List<TroopAI> GetEnemiesOf(TroopFaction faction)
    {
        List<TroopAI> enemies = new List<TroopAI>();

        foreach (var kvp in troopsByFaction)
        {
            if (kvp.Key != faction && kvp.Key != TroopFaction.Neutral)
            {
                enemies.AddRange(kvp.Value);
            }
        }

        return enemies;
    }

    public int GetTroopCount(TroopFaction faction)
    {
        return troopsByFaction[faction].Count;
    }

    public void ClearAll()
    {
        foreach (var list in troopsByFaction.Values)
        {
            list.Clear();
        }
    }
}