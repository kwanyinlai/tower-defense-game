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

            if (Physics.Raycast(ray, out hit, 100f, floorLayerMask))
            {
                // Set the clicked position as node
                Debug.Log("Mouse clicked at: " + GridManager.Instance.NodeFromWorldPos(hit.point));

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

        GridSector enemyTargetSector = gridManager.NodeFromWorldPos(target).gridSector;

        // if we don't have a path, generate this path
        if (highLevelPath == null || highLevelPath.Count == 0)
        {
            
            highLevelPath = SectorManager.Instance.GenerateHighLevelSectorPath(
                currentNode.gridSector,
                enemyTargetSector // TODO: maybe store as attribute in troopAI
            );
            
        }
      
        // remove sectors from the path that we have already reached;
        // we check if they are neighbours as a check for veering off path 
        // so we can regenerate the path if needed
        if (highLevelPath.Count > 0 && highLevelPath[0] != currentNode.gridSector &&
            SectorManager.Instance.SectorAreNeighbours(currentNode.gridSector, highLevelPath[0]))
        {
            highLevelPath.RemoveAt(0);
        }
        
        if (highLevelPath.Count <= 1)
        {
            localTargetNode = gridManager.NodeFromWorldPos(target);
        }
        else if (localTargetNode == null || localTargetNode.gridSector != highLevelPath[0])
        {
            // regenerate a new local target node within the current sector
            Debug.Log("Current Sector: " + currentNode.gridSector.sectorCoordinate + 
                ", Generating new local target node towards sector: " + highLevelPath[0].sectorCoordinate);
            localTargetNode = currentNode.gridSector.GuessOptimalExitNode(
                currentNode,
                highLevelPath[1]
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
        

        Vector3 dirVector;
        // if the current 
        if (highLevelPath.Count <= 1)
        {
            dirVector = (target - transform.position);
            dirVector.y = 0f;
            dirVector.Normalize();
        }
        else
        {
            dirVector = currentNode.gridSector.QueryFlowField(currentNode, localTargetNode, new Vector2(currVelocity.x, currVelocity.y));
            // dirVector.Normalize();
            // dirVector.y *= -1;
            // if (dirVector.y == -1 && localTargetNode.globalY - currentNode.globalY < 0)
            // {
            //     ;
            // }
        }
        



        // check whether the current sector is adjacent to the next target sector
        // if not, regenerate the path because we have veered off path
        if (highLevelPath.Count > 0 && !SectorManager.Instance.SectorAreNeighbours(currentNode.gridSector, highLevelPath[highLevelPath.Count - 1]) &&
            !currentNode.gridSector.Equals(highLevelPath[0]))
        {
            highLevelPath = SectorManager.Instance.GenerateHighLevelSectorPath(
                currentNode.gridSector,
                highLevelPath[highLevelPath.Count - 1]
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
        Vector2 targetVec2 = new Vector2(target.x, target.z);
        Vector2 currPos = new Vector2(transform.position.x, transform.position.z);
        float distanceToTarget = Vector2.Distance(currPos, targetVec2);
        if (distanceToTarget < 1f)
        {
            Debug.Log("Reached target at: " + target);
            enemyTarget = null;
            localTargetNode = null;
            currVelocity = Vector2.zero;
            highLevelPath = null;
            return true;
        }
        return false;
    }


}

