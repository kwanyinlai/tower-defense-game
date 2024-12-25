using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public float projectileSpeed = 10f;
    public int dmg = 10;
    [SerializeField]
    private Transform target;
    private Vector3 targetPos;
    private bool hasHit = false;

    public void SetTarget(Transform bulletTarget)
    {
        Collider collider = bulletTarget.GetComponent<Collider>();
        this.targetPos = bulletTarget.position;
        this.target = bulletTarget;
        targetPos.y += collider.bounds.size.y * (0.75f);
    }

    void Update()
    {
        if (target != null && !hasHit)
        {
            Vector3 dir = (targetPos - transform.position).normalized;
            transform.Translate(dir * projectileSpeed * Time.deltaTime, Space.World);  


            if (Vector3.Distance(transform.position, targetPos) <= 0.1f)
            {
                
                hasHit = true;
                HitTarget();
            }
        }
        else{
            Destroy(gameObject);
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
