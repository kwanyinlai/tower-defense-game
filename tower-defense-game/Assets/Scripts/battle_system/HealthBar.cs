using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class HealthBar : MonoBehaviour
{
    public GameObject target;  
    private BattleSystem health;  
    private Slider healthSlider;
    public Gradient gradient;
    private float timer;
    [SerializeField] private float timeToFade=10f;
    private float prevHP;
    private CanvasGroup opacity;
    private bool fadingInProgress; 
    [SerializeField] private float fadeTime = 10f;
    

    void Start()
    {
        health = target.GetComponent<BattleSystem>();
        healthSlider = GetComponent<Slider>();
        UpdateHealthBar();
        Debug.Log(transform.parent.gameObject.name);
        opacity = transform.parent.gameObject.GetComponent<CanvasGroup>();
    }

    void Update()
    {
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        float healthPercentage = health.GetPercentageHP();
        healthSlider.value = healthPercentage;
       
        if (healthPercentage<1 && timer <= timeToFade)
        {
            timer += Time.deltaTime;
            ShowHealthBar();     
        }
        else if (timer>timeToFade && !fadingInProgress)
        {
            StartCoroutine(FadeOutHealthBar());
        }
        else if (healthPercentage>=1)
        {
            gameObject.transform.localScale = new Vector3(0f,0f,0f);
        }
            
        
        if(prevHP != health.currentHealth)
        {
            prevHP = health.currentHealth;
            ResetTimer();
        }
    }

    void ResetTimer()
    {
        timer = 0f;
    }

    void ShowHealthBar()
    {
        gameObject.transform.localScale = new Vector3(1f,1f, 1f);
        Debug.Log(opacity);
        opacity.alpha = 1f; 
    }


    IEnumerator FadeOutHealthBar()
    {
        fadingInProgress = true;

        
        float startTransparency = opacity.alpha;
        float time = 0f;

        while (time < fadeTime)
        {
            opacity.alpha = Mathf.Lerp(startTransparency, 0f, time / fadeTime);
            time += Time.deltaTime;
            yield return null;
        }

        opacity.alpha = 0f;
 
        gameObject.transform.localScale = Vector3.zero;
        fadingInProgress = false;
    }

    



}
