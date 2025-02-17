using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class TroopMovementRanged : TroopMovement
{
    public GameObject bulletPrefab; 
    public override void Attack()
    {
        transform.LookAt(enemyTarget);
        Vector3 bulletPos = transform.position;
        Collider collider = this.GetComponent<Collider>();
        bulletPos.y += collider.bounds.size.y * (0.75f);
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletPos, transform.rotation);
            BulletScript bulletScript = bullet.GetComponent<BulletScript>();
            if (bulletScript != null)
            {
                bulletScript.SetExceptionList(exceptionBulletList);
                bulletScript.SetTarget(enemyTarget);
            }
        }
        atkTimer = atkCooldown;
    }

}



