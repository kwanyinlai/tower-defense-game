using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public float projectileSpeed = 10f;         
    public int dmg = 10;          
    public float lifetime = 5f;    
    private Transform target;        
    private bool hasHit = false;      

    void Start()
    {
       
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    void Update()
    {
        if (target != null && !hasHit)
        {
           
            Vector3 dir = (target.position - transform.position).normalized;
            transform.Translate(dir * projectileSpeed * Time.deltaTime, Space.World);  


            if (Vector3.Distance(transform.position, target.position) <= 0.1f)
            {
                
                hasHit = true;
                HitTarget();
            }
        }
        else{
             Destroy(gameObject, lifetime);
        }
    }


    private void HitTarget()
    {
        BattleSystem battleSystem = target.GetComponent<BattleSystem>();
        if (battleSystem != null)
        {
            battleSystem.TakeDamage(dmg);
        }

        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform == target && !hasHit)
        {
            HitTarget();
        }
    }
}
