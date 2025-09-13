using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ResourceBuilding : Building
{
    protected float collectionInterval = 3f;
    protected float timer = 0f;
    [SerializeField]private GameObject resourceNode;


    new protected void Start()
    {
        base.Start();
        foreach(Transform child in GameObject.Find("Resource Nodes").transform)
        {
            if(child.gameObject.activeInHierarchy && StaticScripts.horizontalDistance(child.position.x, child.position.y, 
                transform.position.x, transform.position.y) < range)
            {
                resourceNode = child.gameObject;
                break;
            }
        }
    }

    protected override void IntializeSellResources()
    {
        
    }

    protected void Update()
    {
        timer += Time.deltaTime;
        if (timer >= collectionInterval && resourceNode != null)
        {
            timer = 0f;
            resourceNode.GetComponent<ResourceNode>().CollectResources();
        }
    }

    
}
