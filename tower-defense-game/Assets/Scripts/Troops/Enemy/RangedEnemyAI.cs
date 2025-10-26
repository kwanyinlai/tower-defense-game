using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.Serialization.Json;

public class RangedEnemyAI : IAttackBehaviour
{
    public GameObject bulletPrefab;
    private HashSet<string> exceptionBulletList = new HashSet<string>{"Enemy"};
    
}
