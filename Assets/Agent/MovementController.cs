//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GameAISimulator.Enums;

[RequireComponent(typeof(PhysicsObject))]
public class MovementController : MonoBehaviour
{
    public bool isMovingToPosition;
    public float forceMultiplier = 10f;
    public float timeToMaxWalkingSpeed = 1f;
    Vector3 moveTargetPosition;
    Vector3 moveForce;
    public bool isFinalMovePosition = false;

    [Header("Path Modifiers")]
    [SerializeField] bool canJump;
    [SerializeField] bool isPenalizingDeviations;
    [SerializeField] bool isPathSmoothed;
    [SerializeField] bool isCollinearPointsRemoved;
    [SerializeField] float jumpArcHeight = 3f;

    [SerializeField] bool isHierarchicalSearch;

    public void Toggle_CanJump(bool isEnabled)
    {
        canJump = isEnabled;
    }
    public void Toggle_IsPenalizingDeviations(bool isEnabled)
    {
        isPenalizingDeviations = isEnabled;
    }
    public void Toggle_IsPathSmoothed(bool isEnabled)
    {
        isPathSmoothed = isEnabled;
    }
    public void Toggle_IsCollinearPointsRemoved(bool isEnabled)
    {
        isCollinearPointsRemoved = isEnabled;
    }

    public void Toggle_IsHierarchicalSearch(bool isEnabled)
    {
        isHierarchicalSearch = isEnabled;
    }

    [SerializeField] bool isShowingPathPoints;
    private bool isVisualizingPath;
    public bool IsVisualizingPath
    {
        get { return isVisualizingPath; }
        set
        {
            isVisualizingPath = value;
            Toggle_IsShowingPathPoints(isVisualizingPath);
            lineRenderer.enabled = isVisualizingPath;
        }
    }

    public void Toggle_IsShowingPathPoints(bool isEnabled)
    {
        isShowingPathPoints = isEnabled;

        if (currentPath != null)
        {
            if (isShowingPathPoints)
            {
                CreatePathPoints(currentPath);
            }

            for (int i = 0; i < pathPointMarkers.Count; i++)
            {
                pathPointMarkers[i].SetActive(isShowingPathPoints);
            }
        }
    }

    [SerializeField] GameObject pathPointMarkerPrefab;

    Node goalNode;
    Node prevGoalNode;
    CellGrid cellGrid;

    PhysicsObject physicsObject;

    Pathfinder pathfinder;

    private Path currentPath;
    Path CurrentPath
    {
        set
        {
            currentPath = value;
            numPathPoints = currentPath.points.Count;
        }
    }

    [SerializeField] LineRenderer lineRenderer;

    public void Initialize(CellGrid cellGrid, Pathfinder pathfinder)
    {
        physicsObject = GetComponent<PhysicsObject>();
        this.cellGrid = cellGrid;
        this.pathfinder = pathfinder;
    }

    bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public void EnableAllPathingImprovements()
    {
        isHierarchicalSearch = isPenalizingDeviations = isPathSmoothed = isCollinearPointsRemoved = true;
    }

    public void PathToCell(Vector3 position)
    {
        Node possibleGoalNode = cellGrid.GetCell(position).node;
        PathToNode(possibleGoalNode);
    }
    public void PathToCell(Cell cell)
    {
        Node possibleGoalNode = cell.node;
        PathToNode(possibleGoalNode);
    }

    private void PathToNode(Node possibleGoalNode)
    {
        if (possibleGoalNode != prevGoalNode) // If path has not already been calculated.
        {
            Node possibleStartNode = cellGrid.GetCell(new Vector3(transform.position.x, 0, transform.position.z)).node;

            if (possibleStartNode != possibleGoalNode) // If not attempting to path to agent's current node.
            {
                Path possiblePath = pathfinder.GetPath(possibleStartNode, possibleGoalNode, isHierarchicalSearch, canJump, isPenalizingDeviations, isPathSmoothed, isCollinearPointsRemoved);

                if (possiblePath != null)
                {
                    CurrentPath = possiblePath;
                    goalNode = possibleGoalNode;
                    prevGoalNode = goalNode;

                    if (isVisualizingPath)
                    {
                        VisualizePath(currentPath);
                    }

                    StartPathing(currentPath);
                }
            }
        }
    }

    List<GameObject> pathPointMarkers = new List<GameObject>();

    void VisualizePath(Path path)
    {
        lineRenderer.positionCount = numPathPoints;

        Vector3[] lineRenderPositions = new Vector3[numPathPoints];
        for (int i = 0; i < numPathPoints; i++)
        {
            lineRenderPositions[i] = path.points[i].position + DebugMetrics.lineRendererOffsetY;
        }

        lineRenderer.SetPositions(lineRenderPositions);

        if (isShowingPathPoints)
        {
            CreatePathPoints(path);
        }
    }

    void CreatePathPoints(Path path)
    {
        for (int i = 0; i < pathPointMarkers.Count; i++)
        {
            Destroy(pathPointMarkers[i]);
        }

        pathPointMarkers.Clear();

        for (int i = 0; i < numPathPoints; i++)
        {
            GameObject marker = Instantiate(pathPointMarkerPrefab);
            marker.transform.position = path.points[i].position + DebugMetrics.lineRendererOffsetY;
            pathPointMarkers.Add(marker);
        }
    }

    public void MoveTo(Vector3 targetPosition)
    {
        moveTargetPosition = targetPosition;
        Vector3 moveDirection = (moveTargetPosition - transform.position).normalized;
        moveForce = moveDirection * forceMultiplier / timeToMaxWalkingSpeed;
        //isMovingToPosition = true;
    }

    public bool isPathing;
    int currentPathPointIndex;
    int numPathPoints;
    bool isPathingToFinalPoint;

    public void StartPathing(Path path)
    {
        currentPathPointIndex = numPathPoints - 2; // The agent is assumed to already be at or close enough to the first point.
        isPathing = true;
        if (currentPathPointIndex == 0)
        {
            isPathingToFinalPoint = true;
        }
        else
        {
            isPathingToFinalPoint = false;
        }
        MoveTo(currentPath.points[currentPathPointIndex].position);
    }

    public void ToggleJumping()
    {
        canJump = !canJump;
    }

    public void FixedUpdate_MovementController()
    {
        if (isPathing)
        {
            MoveTo(currentPath.points[currentPathPointIndex].position);

            if (isPathingToFinalPoint)
            {
                if (IsCloseTo(moveTargetPosition, forceMultiplier / 2))
                {
                    if (IsCloseTo(moveTargetPosition, 0.1f))
                    {
                        physicsObject.Stop();
                        isPathing = false;
                        isPathingToFinalPoint = false;
                    }
                }
            }
            else
            {
                if (IsCloseTo(moveTargetPosition, 0.1f))
                {
                    currentPathPointIndex--;
                    if (currentPathPointIndex == 0)
                    {
                        isPathingToFinalPoint = true;
                    }

                    if (canJump)
                    {
                        if (currentPath.points[currentPathPointIndex].pathPointType == PathPointType.Edge)
                        {
                            currentPathPointIndex--;
                            if (currentPathPointIndex == 0)
                            {
                                isPathingToFinalPoint = true;
                            }
                            else
                            {
                                while (currentPath.points[currentPathPointIndex].pathPointType == PathPointType.Curve)
                                {
                                    currentPathPointIndex--;
                                }
                            }

                            physicsObject.Stop();
                            moveTargetPosition = currentPath.points[currentPathPointIndex].position;
                            moveForce = Vector3.zero;
                            physicsObject.Launch(moveTargetPosition, jumpArcHeight);
                        }
                    }
                    else
                    {
                        MoveTo(currentPath.points[currentPathPointIndex].position);
                    }
                }
            }

            if (physicsObject.isGrounded)
            {
                physicsObject.ApplyForce(moveForce);
            }

            physicsObject.UpdateThisShit();

            transform.forward = physicsObject.velocity.normalized;
        }
    }

    public bool IsCloseTo(Vector3 targetPosition, float distance)
    {
        if (Vector3.Distance(transform.position, targetPosition) <= distance)
        {
            return true;
        }

        return false;
    }

    public void Stop()
    {
        isPathing = false;
        isFinalMovePosition = false;
    }
}
