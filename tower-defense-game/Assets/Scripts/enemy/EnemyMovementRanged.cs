using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;

public class EnemyMovementRanged : EnemyMovement
{
    public GameObject bulletPrefab;
    private HashSet<string> exceptionBulletList = new HashSet<string>{"Enemy"};
    
    public override void Attack(Transform bulletTarget)
    {

        transform.LookAt(bulletTarget); 
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w); // essentially only allows the y axis to move

        
        Collider collider = this.GetComponent<Collider>();
        Vector3 bulletPos = transform.position + new Vector3(0f, collider.bounds.size.y * (0.75f), 0); // Adds Vector For Y Axis To Shift Bullet To Head Level
        // bulletPos.y += collider.bounds.size.y * (0.75f);
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletPos, transform.rotation);
            BulletScript bulletScript = bullet.GetComponent<BulletScript>();
            if (bulletScript != null)
            {
                bulletScript.SetExceptionList(exceptionBulletList);
                bulletScript.SetTarget(bulletTarget);
            }
        }
        atkTimer = atkCooldown;
    }

}
