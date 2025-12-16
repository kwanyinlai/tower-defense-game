using UnityEngine;
using System.Collections.Generic;


public class TroopSelectorRadius : MonoBehaviour
{
    private List<GameObject> troopsInRadius = new List<GameObject>();
    private bool isAcceptingCollisions = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider collision)
    {
        GameObject obj = collision.gameObject;
        if(obj.CompareTag("Troop")) {
            troopsInRadius.Add(obj);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        GameObject obj = collision.gameObject;
        if(obj.CompareTag("Troop")) {
            troopsInRadius.Remove(obj);
        }
    }

    public List<GameObject> GetTroopsInRadius() {
        return troopsInRadius;
    }

    public void ClearTroopsInRadius() {
        troopsInRadius.Clear();
    }

    public void SetIsAcceptingCollisions(bool isAccepting) {
        isAcceptingCollisions = isAccepting;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
