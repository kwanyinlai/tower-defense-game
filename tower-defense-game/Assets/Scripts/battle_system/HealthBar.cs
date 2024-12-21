using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public GameObject target;  
    private BattleSystem health;  
    private Slider healthSlider;
    public Gradient gradient;

    void Start()
    {   
        health = target.GetComponent<BattleSystem>();
        healthSlider = GetComponent<Slider>();
        UpdateHealthBar();
        target = gameObject;
    }

    void Update()
    {
        UpdateHealthBar();
        
    }

    void UpdateHealthBar()
    {
        float healthPercentage = health.GetPercentageHP();
        healthSlider.value = healthPercentage;
    }


}
