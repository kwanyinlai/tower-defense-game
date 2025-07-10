using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class RangedTroopAI : TroopAI
{
    public GameObject bulletPrefab; 
    public override void Attack(Transform target)
    {
        transform.LookAt(enemyTarget);
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w); // essentially only allows the y axis to move
        
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



