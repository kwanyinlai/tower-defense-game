using UnityEngine;
using System.Collections.Generic;

public class TroopManagment : MonoBehaviour
{

    public float controlRadius;
    [SerializeField] private GameObject selectorCircle;
    public static List<GameObject> selected = new List<GameObject>();
    private bool selecting;
    private float selectionRadius=10f;

    
    void Start()
    {
        if(Input.GetKeyDown("space")){
            selecting = !selecting;
            SelectTroops();
        }
    }

    // Update is called once per frame
    void Update()
    {
        SelectTroops();
    }

    void SelectTroops(){
                
        foreach (GameObject troop in PlayerMovement.troops)
        {
            Vector3 playerPosition = transform.position;
            Vector3 troopPosition = troop.transform.position;
            playerPosition.y = 0f;
            troopPosition.y = 0f;

            Debug.Log("COntains:" +PlayerMovement.troops.Contains(troop));
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
