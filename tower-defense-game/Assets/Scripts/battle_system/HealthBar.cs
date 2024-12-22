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
    }

    void Update()
    {
        UpdateHealthBar();
        
    }

    void UpdateHealthBar()
    {
        float healthPercentage = health.GetPercentageHP();
        healthSlider.value = healthPercentage;
       
        if (healthPercentage<1){
            gameObject.transform.localScale = new Vector3(1f,1f, 1f);
        }
        else{
           gameObject.transform.localScale = new Vector3(0,0,0);
        }
    }


}
