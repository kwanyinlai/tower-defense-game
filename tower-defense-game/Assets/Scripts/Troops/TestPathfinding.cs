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
    [SerializeField] protected Vector3? enemyTarget;

    

    private void Update()
    {
        HandleMouseInput();

        if (enemyTarget != null)
        {
            MoveTowardsTarget(enemyTarget.Value);
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            

            int floorLayerMask = LayerMask.GetMask("Floor");
            Debug.Log("Shooting ray at mouse position");

            if (Physics.Raycast(ray, out hit, 100f, floorLayerMask))
            {
                // Set the clicked position as target
                Debug.Log("Mouse clicked at: " + hit.point);
                Vector3 clickPos = hit.point;
                clickPos.y = 0; // Assuming a 2D plane at z=0
                enemyTarget = clickPos;
            }
        }
    }



    protected void MoveTowardsTarget(Vector3 target)
    { 
        if (CheckReachedTarget(target))
        {
            return;
        }
        GridManager gridManager = GridManager.Instance;

        GridNode currentNode = gridManager.NodeFromWorldPos(transform.position);

        localTargetNode = gridManager.NodeFromWorldPos(target);


        if (highLevelPath == null)
        {   
            highLevelPath = SectorManager.Instance.GenerateHighLevelSectorPath(
                currentNode.gridSector,
                localTargetNode.gridSector // TODO: maybe store as attribute in troopAI
            );
        }
       
        Debug.Log($"Current length of path {highLevelPath.Count}");
        Debug.Log($"Last sector in path {highLevelPath[highLevelPath.Count - 1].sectorCoordinate.x}, {highLevelPath[highLevelPath.Count-1].sectorCoordinate.y}");
        Debug.Log($"First sector in path {highLevelPath[0].sectorCoordinate.x}, {highLevelPath[0].sectorCoordinate.y}");
        Debug.Log($"Last sector is our node  {currentNode.gridSector == highLevelPath[highLevelPath.Count-1]}");
        Debug.Log($"First sector is our current node {currentNode.gridSector == highLevelPath[0]}");
        
        if (highLevelPath.Count > 0 && highLevelPath[0] == currentNode.gridSector)
        {
            Debug.Log("why are we removing");
            highLevelPath.RemoveAt(0);
        }
        
        if (highLevelPath.Count == 0)
        {
            localTargetNode = gridManager.NodeFromWorldPos(target);
        }
        else
        {
            localTargetNode = currentNode.gridSector.GuessOptimalExitNode(
                currentNode,
                highLevelPath[0]
            );
            // if (highLevelPath.Count > 1)
            // {
            //     localTargetNode = currentNode.gridSector.GuessOptimalExitNode(
            //         currentNode,
            //         highLevelPath[0],
            //         highLevelPath[1]
            //     );
            // }
            // else
            // {
            //     localTargetNode = currentNode.gridSector.GuessOptimalExitNode(
            //         currentNode,
            //         highLevelPath[0]
            //     );
            // }
        }
        

        
        Vector3 delta;
        if  (highLevelPath.Count < 1)
        {
            delta = (target - transform.position);
        }
        else{
            delta = currentNode.gridSector.QueryFlowField(currentNode, localTargetNode);
            Debug.Log("I'm querying from the flow field");
        }

        Vector2 dirVector = new Vector2(delta.x, delta.z).normalized;
        Debug.Log($"DIR VECTOR = {dirVector}");

        // check whether the current sector is adjacent to the next target sector
        // if not, regenerate the path because we have veered off path
        if (highLevelPath.Count > 0 && !SectorManager.Instance.SectorAreNeighbours(currentNode.gridSector, highLevelPath[highLevelPath.Count-1]))
        {
            highLevelPath = SectorManager.Instance.GenerateHighLevelSectorPath(
                currentNode.gridSector,
                localTargetNode.gridSector // TODO: maybe store as attribute in troopAI
            );
            return;
        }

        // steering behaviours

       
        

        currVelocity = Vector2.MoveTowards(currVelocity, dirVector.normalized * maxSpeed, acceleration * Time.deltaTime);

        transform.position += new Vector3(currVelocity.x, 0f, currVelocity.y) * Time.deltaTime;

        if (currVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(currVelocity.y, currVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }


        

    }

    private bool CheckReachedTarget(Vector3 target)
    {   
        float distanceToTarget = Vector3.Distance(transform.position, target);
        if (distanceToTarget < 0.1f)
        {
            Debug.Log("Reached target at: " + target);
            enemyTarget = null;
            currVelocity = Vector2.zero;
            highLevelPath = null;
            return true;
        }
        return false;
    }


}

