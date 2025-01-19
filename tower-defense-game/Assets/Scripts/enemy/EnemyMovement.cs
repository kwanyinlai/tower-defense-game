using UnityEngine;
using System.Collections.Generic;
public abstract class EnemyMovement : MonoBehaviour
{
    public static List<GameObject> enemies = new List<GameObject>(); 
    public abstract void Attack(Transform target);
    public abstract Transform GetClosestEnemyInRange();
}
