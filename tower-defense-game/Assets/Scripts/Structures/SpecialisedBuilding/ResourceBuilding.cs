using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ResourceBuilding : Building
{
    protected float collectionInterval = 3f;
    protected float timer = 0f;
    public string nodeType;
    [SerializeField]private GameObject resourceNode;
    ResourceNode nodeScript;


    new protected void Start()
    {
        base.Start();
        Transform allNodes = GameObject.Find("Resource Nodes").transform;
        foreach(Transform child in allNodes)
        {
            if(child.gameObject.activeInHierarchy && child.gameObject.name == nodeType &&
                Vector2.Distance(child.position, transform.position) < range)
            {
                resourceNode = child.gameObject;
                nodeScript = resourceNode.GetComponent<ResourceNode>();
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
            nodeScript.CollectResources();
        }
    }

    
}
