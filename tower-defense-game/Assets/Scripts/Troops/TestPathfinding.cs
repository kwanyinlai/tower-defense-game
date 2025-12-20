using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;


public class TestPathfinding : MonoBehaviour
{

    [SerializeField] protected float maxSpeed = 3.5f;
    public float MaxSpeed { get { return maxSpeed; } }


    public Vector2 currVelocity = Vector2.zero;
    public float acceleration = 5f;
    private List<GridSector> highLevelPath;
    private GridNode localTargetNode;

  
    // Combat Stats
    [SerializeField] protected Vector3 enemyTarget;

    

    private void Update()
    {
        HandleMouseInput();

        if (enemyTarget != null)
        {
            MoveTowardsTarget(enemyTarget);
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            int floorLayerMask = LayerMask.GetMask("Floor");

            if (Physics.Raycast(ray, out hit, 100f, floorLayerMask))
            {
                // Set the clicked position as target
                Debug.Log("Mouse clicked at: " + hit.point);
                Vector3 clickPos = hit.point;
                clickPos.z = 0f; // Ensure movement in 2D plane
                enemyTarget = clickPos;
            }
        }
    }



    protected void MoveTowardsTarget(Vector3 target)
    { 
        Debug.Log("Moving towards target: " + target);
        GridManager gridManager = GridManager.Instance;

        GridNode currentNode = gridManager.NodeFromWorldPos(target);
        localTargetNode = gridManager.NodeFromWorldPos(target);

        if (highLevelPath == null)
        {   
            Debug.Log("Current Node Coordinate: " + currentNode.globalX + ", " + currentNode.globalY);
            Debug.Log("Current Node Sector: " + currentNode.gridSector);
            Debug.Log("Current Node Sector is not null: " + (currentNode.gridSector != null));
            highLevelPath = SectorManager.Instance.GenerateHighLevelSectorPath(
                currentNode.gridSector,
                localTargetNode.gridSector // TODO: maybe store as attribute in troopAI
            );
        }
       
        
        if (highLevelPath.Count > 0 && highLevelPath[0] == currentNode.gridSector)
        {
            highLevelPath.RemoveAt(0);
            if (highLevelPath.Count == 0)
            {
                localTargetNode = gridManager.NodeFromWorldPos(target);
            }
            else
            {
                if (highLevelPath.Count > 1)
                {
                    localTargetNode = currentNode.gridSector.GuessOptimalExitNode(
                        currentNode,
                        highLevelPath[0],
                        highLevelPath[1]
                    );
                }
                else
                {
                    localTargetNode = currentNode.gridSector.GuessOptimalExitNode(
                        currentNode,
                        highLevelPath[0]
                    );
                }
            }
        }

        Vector2 dirVector;
        if  (highLevelPath.Count == 0)
        {
            dirVector = (target - transform.position).normalized;
        }
        else{
            dirVector = currentNode.gridSector.QueryFlowField(currentNode, localTargetNode);
        }

        // check whether the current sector is adjacent to the next target sector
        // if not, regenerate the path because we have veered off path
        if (highLevelPath.Count > 0 && !SectorManager.Instance.SectorAreNeighbours(currentNode.gridSector, highLevelPath[0]))
        {
            highLevelPath = SectorManager.Instance.GenerateHighLevelSectorPath(
                currentNode.gridSector,
                localTargetNode.gridSector // TODO: maybe store as attribute in troopAI
            );
            return;
        }

        // steering behaviours

       
        

        currVelocity = Vector2.MoveTowards(currVelocity, dirVector.normalized * maxSpeed, acceleration * Time.deltaTime);

        transform.position += (Vector3)(currVelocity * Time.deltaTime);

        if (currVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(currVelocity.y, currVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }


        

    }


}

