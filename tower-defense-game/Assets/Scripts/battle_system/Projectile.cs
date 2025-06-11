using UnityEngine;
using System.Collections.Generic;

public class BulletScript : MonoBehaviour
{
    public float projectileSpeed = 10f;
    public int dmg = 10;

    [SerializeField]
    private Transform target;
    private Vector3 targetPos;
    private bool hasHit = false;

    private HashSet<string> exceptionList; // List of Tags that the Bullet Cannot Hit, Uses a Set for Efficiency When Looking At Overlaps


    public void SetTarget(Transform bulletTarget)
    {
        Collider collider = bulletTarget.GetComponent<Collider>();
        this.targetPos = bulletTarget.position;
        this.target = bulletTarget;
        Vector3 rot = transform.rotation.eulerAngles;
        targetPos.y += collider.bounds.size.y * (0.75f);
        rot.x -= Mathf.Atan2((targetPos.y - transform.position.y), Mathf.Sqrt(
                Mathf.Pow(targetPos.x - transform.position.x, 2) + Mathf.Pow(
                targetPos.z - transform.position.z, 2))) * 180 / Mathf.PI;
        transform.rotation = Quaternion.Euler(rot);
        
    }

    void Update()
    {
        // Check if target is still alive
        if (target != null && !hasHit)
        {
            Vector3 dir = (targetPos - transform.position).normalized;
            transform.Translate(dir * projectileSpeed * Time.deltaTime, Space.World);  

            // Failsafe for when target has moved from original posistion
            if (Vector3.Distance(transform.position, targetPos) <= 0.1f)
            {
                Destroy(gameObject);
            }
        }
        else{
            Destroy(gameObject);
        }
    }


    private void HitTarget()
    {
        CombatSystem combatSystem = target.GetComponent<CombatSystem>();
        if (combatSystem != null)
        {
            combatSystem.TakeDamage(dmg);
        }

        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        CombatSystem sys = collision.gameObject.GetComponent<CombatSystem>();
        // Should only hit target if it is a hitable object (aka with a BattleSystem)
        if(sys != null && exceptionList != null)
        {
            // Overlaps is very efficient and thus why using HashSets
            if(!sys.GetTagList().Overlaps(exceptionList))
            {
                HitTarget();
            }
        }
    }

    public void SetExceptionList(HashSet<string> tags)
    {
        exceptionList = tags;
    }
}
