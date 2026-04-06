using UnityEngine;

public class BuildingCombatSystem : CombatSystem
{
    [Header("Building Properties")]
    [SerializeField] private float armor = 5f;
    [SerializeField] private bool isInvulnerable = false; // invulnerability maybe for certain buildings
    // so exclusion from targetting or what not
    
    [Header("Destruction")]
    [SerializeField] private GameObject destructionEffectPrefab;
    [SerializeField] private GameObject[] debrisObjects;
    [SerializeField] private float debrisLifetime = 30f;

    #region Damage & Defense

    public override void TakeDamage(float damage)
    {
        if (isInvulnerable) return;

        // Apply armor reduction
        float reducedDamage = Mathf.Max(1f, damage - armor);

        base.TakeDamage(reducedDamage);
    }

    public float GetArmorValue() => armor;

    public void AddArmor(float amount) => armor += amount;

    #endregion

    #region Destruction

    protected override void Die()
    {
        OnBuildingDestroyed();
        
        if (destructionEffectPrefab != null)
        {
            Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
        }

        SpawnDebris();
        base.Die();
    }

    private void SpawnDebris()
    {
        if (debrisObjects == null || debrisObjects.Length == 0) return;

        foreach (GameObject debrisPrefab in debrisObjects)
        {
            if (debrisPrefab == null) continue;

            Vector3 randomOffset = Random.insideUnitSphere * 2f;
            randomOffset.y = 0;
            
            GameObject debris = Instantiate(
                debrisPrefab,
                transform.position + randomOffset,
                Quaternion.Euler(0, Random.Range(0, 360), 0)
            );

            Destroy(debris, debrisLifetime);
        }
    }

    protected virtual void OnBuildingDestroyed()
    {
        Debug.Log($"Building {gameObject.name} has been destroyed!");
    }

    #endregion

    #region Status Effects

    protected override void InitializeEffects()
    {
        base.InitializeEffects();
    }

    public bool IsDisabled() => HasEffect(StatusClass.Disabled);

    #endregion
}