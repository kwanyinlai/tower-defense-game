using UnityEngine;
using System.Collections.Generic;

public class TroopManagment : MonoBehaviour
{

    [SerializeField] private GameObject selectorCircle;
    public static List<GameObject> selected = new List<GameObject>();
    private bool selecting;
    public float selectionRadius = 8f;

    
    void Start()
    {
        selecting = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("space")){
            selecting = !selecting;

            
            
        }
        if(selecting){
            SelectTroops();
            selectorCircle.transform.localScale = new Vector3(15f,0.2f,15f);
        }
        else{
            selectorCircle.transform.localScale = new Vector3(0,0,0);
            selected.Clear();
        }
    }

    void SelectTroops(){
                
        foreach (GameObject troop in PlayerMovement.troops)
        {
            Vector3 playerPosition = transform.position;
            Vector3 troopPosition = troop.transform.position;
            playerPosition.y = 0f;
            troopPosition.y = 0f;

            Debug.Log("Distance = " + Vector3.Distance(playerPosition, troopPosition) );
            if (Vector3.Distance(playerPosition, troopPosition) <= selectionRadius && 
                    !selected.Contains(troop))
            {
                selected.Add(troop);
            }
        }

        Debug.Log("Selected Troops: " + selected.Count);
    }
}
