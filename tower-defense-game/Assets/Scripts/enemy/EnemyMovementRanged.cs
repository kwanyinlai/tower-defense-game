using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;

public class EnemyMovementRanged : EnemyMovement
{
    public GameObject bulletPrefab;
    
    public override void Attack(Transform bulletTarget)
    {
        transform.LookAt(bulletTarget);
        Vector3 bulletPos = transform.position;
        Collider collider = this.GetComponent<Collider>();
        bulletPos.y += collider.bounds.size.y * (0.75f);
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletPos, transform.rotation);
            BulletScript bulletScript = bullet.GetComponent<BulletScript>();
            if (bulletScript != null)
            {
                bulletScript.SetTarget(bulletTarget);
            }
        }
        atkTimer = atkCooldown;
    }

}
