//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. "A* Speed Optimizations" by Steve Rabin in Game Programming Gems(Book)
//  2. "A* Aesthetic Optimizations" by Steve Rabin in Game Programming Gems(Book)
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using GameAISimulator.Enums;

public class Pathfinder : MonoBehaviour
{
    /// <summary> Unsearched nodes waiting in the priority queue. </summary>
    PriorityQueue<Node> openList;
    /// <summary> Fully searched nodes. </summary>
    List<Node> closedList;

    float heuristicWeight = 1f;
    public int pathTestIterations = 10;
    [SerializeField] bool isApplyingCatmullRomFormula;

    [SerializeField] NodeGraph nodeGraph;
    [SerializeField] RegionManager regionManager;

    public Path GetPath(Node start, Node goal, bool isHierarchicalSearch, bool isJumping, bool isPenalizingDeviations, bool isSmoothed, bool isCollinearPointsRemoved)
    {
        if (isHierarchicalSearch)
        {
            FindRegionPath(regionManager.Regions[start.Cell.regionID], regionManager.Regions[goal.Cell.regionID], isJumping);
        }

        Path path = FindPath(start, goal, isHierarchicalSearch, isJumping, isPenalizingDeviations);

        if (path != null)
        {
            if (isSmoothed)
            {
                path = SmoothPath(path);
            }

            if (isCollinearPointsRemoved)
            {
                path = RemoveCollinearPoints(path);
            }
        }

        return path;
    }

    public void Initialize()
    {
        openList = new PriorityQueue<Node>();
        closedList = new List<Node>();
    }

    void Reset()
    {
        openList.Clear();
        closedList.Clear();
        nodeGraph.ResetAllSearchData();
    }

    public Path FindPath(Node start, Node goal, bool isHierarchicalSearch, bool isJumping, bool isPenalizingDeviations)
    {
        Reset();
        Path path = new Path();

        start.gCost = 0;
        start.hCost = Mathf.Abs(
                            Vector3.Distance(start.position, goal.position)
                            );
        start.fCost = start.hCost;

        openList.Enqueue(start, start.fCost);

        start.SearchStatus = SearchStatus.OpenList;

        while (!openList.IsEmpty())
        {
            Node bestNode = openList.Dequeue();
            if (bestNode == goal)
            {
                //Construct Path
                Node pathNode = bestNode;
                while (pathNode != start)
                {
                    path.AddPoint(new PathPoint(PathPointType.Node, pathNode, null, pathNode.position));
                    path.AddPoint(new PathPoint(PathPointType.Edge, null, pathNode.edgeOfParent, pathNode.edgeOfParent.Position));
                    //pathNode.GetCell().
                    //Debug.Log(pathNode.position + " Added to path!");
                    pathNode = pathNode.parent;
                }
                path.AddPoint(new PathPoint(PathPointType.Node, start, null, start.position));

                return path;
            }
            else
            {
                Node neighbor;

                // In Support of Straight Paths
                float nonStraightPenalty = 0f;
                bool isTherePreviousEdge = false;
                Direction previousEdgeDirection = Direction.None;
                if (isPenalizingDeviations)
                {
                    if (bestNode.edgeOfParent != null)
                    {
                        isTherePreviousEdge = true;
                        previousEdgeDirection = bestNode.edgeOfParent.DirectionFromNodeOne;
                    }
                    else
                    {
                        isTherePreviousEdge = false;
                    }
                }

                //Debug.Log("BestNode position: " + bestNode.position);
                //Debug.Log("Bestnode # edges= " + bestNode.edges.Count);
                for (int i = 0; i < bestNode.Edges.Count; i++)
                {
                    neighbor = bestNode.Edges[i].OtherNode(bestNode);

                    // In Support of Straight Paths
                    if (isPenalizingDeviations && isTherePreviousEdge)
                    {
                        if (previousEdgeDirection == bestNode.Edges[i].DirectionFromNodeOne) {
                            nonStraightPenalty = 0f;
                        } else {
                            nonStraightPenalty = bestNode.Edges[i].Cost * 0.5f;
                        }
                    }

                    if (isHierarchicalSearch)
                    {
                        if (regionManager.Regions[neighbor.Cell.regionID].SearchStatus != SearchStatus.OnPath)
                        {
                            continue;
                        }
                    }

                    if (neighbor.SearchStatus == SearchStatus.None)
                    {
                        if (bestNode.Edges[i].EdgeType == EdgeType.Elevated)
                        {
                            if (!isJumping) // If jumping is not allowed, maximize the cost of nodes accessed through elevation nodes.
                            {
                                continue;
                                //neighbor.gCost = Mathf.Infinity;
                            }
                            else //If jumping is allowed, simply calcualte costs as normal.
                            {
                                neighbor.gCost = bestNode.gCost + bestNode.Edges[i].Cost;
                            }
                        }
                        else
                        {
                            neighbor.gCost = bestNode.gCost + bestNode.Edges[i].Cost;
                        }

                        neighbor.hCost = Mathf.Abs( Vector3.Distance(neighbor.position, goal.position) ) * heuristicWeight;
                        neighbor.fCost = neighbor.gCost + neighbor.hCost + nonStraightPenalty;
                        neighbor.parent = bestNode;
                        neighbor.edgeOfParent = bestNode.Edges[i];

                        openList.Enqueue(neighbor, neighbor.fCost);

                        neighbor.SearchStatus = SearchStatus.OpenList;
                    }
                    else if (neighbor.SearchStatus == SearchStatus.OpenList)
                    {
                        float gCost = bestNode.gCost + bestNode.Edges[i].Cost;
                        float fCost = gCost + neighbor.hCost + nonStraightPenalty;

                        if (fCost < neighbor.fCost)
                        {
                            neighbor.gCost = gCost;
                            neighbor.fCost = fCost;
                            neighbor.parent = bestNode;
                            neighbor.edgeOfParent = bestNode.Edges[i];
                        }
                    }
                    else if (neighbor.SearchStatus == SearchStatus.ClosedList)
                    {
                        continue;
                    }
                }
                closedList.Add(bestNode);
                bestNode.SearchStatus = SearchStatus.ClosedList;
            }
        }

        Debug.LogWarning("Null path has been returned!");
        return null;
    }

    public RegionPath FindRegionPath(Region start, Region goal, bool isJumping)
    {
        regionManager.ResetAllSearchData();

        /// <summary> Unsearched regions waiting in the priority queue. </summary>
        PriorityQueue<Region> regionOpenList = new PriorityQueue<Region>();
        /// <summary> Fully searched regions. </summary>
        List<Region> closedList = new List<Region>();

        start.gCost = 0;
        start.hCost = Mathf.Abs(
                            Vector3.Distance(start.centerPosition, goal.centerPosition)
                            );
        start.fCost = start.hCost;

        regionOpenList.Enqueue(start, start.fCost);

        start.SearchStatus = SearchStatus.OpenList;

        while (!regionOpenList.IsEmpty())
        {
            Region bestRegion = regionOpenList.Dequeue();
            if (bestRegion == goal)
            {
                //Construct Path
                RegionPath regionPath = new RegionPath();
                Region regionOnPath = bestRegion;
                while (regionOnPath != start)
                {
                    regionPath.AddPoint(new RegionPathPoint(RegionPathPointType.Region, regionOnPath, null, regionOnPath.centerPosition));
                    regionOnPath.SearchStatus = SearchStatus.OnPath;
                    regionPath.AddPoint(new RegionPathPoint(RegionPathPointType.Portal, null, regionOnPath.portalOfParent, regionOnPath.portalOfParent.position));
                    regionOnPath = regionOnPath.parent;
                }
                regionPath.AddPoint(new RegionPathPoint(RegionPathPointType.Region, start, null, start.centerPosition));
                regionOnPath.SearchStatus = SearchStatus.OnPath;

                return regionPath;
            }
            else
            {
                Region neighbor;

                for (int i = 0; i < bestRegion.portals.Count; i++)
                {
                    neighbor = bestRegion.portals[i].OtherRegion(bestRegion);
                    if (neighbor.SearchStatus == SearchStatus.None)
                    {
                        if (bestRegion.portals[i].EdgeType == EdgeType.Elevated)
                        {
                            if (!isJumping) // If jumping is not allowed, ignore this region and start analyzing the next.
                            {
                                continue;
                            }
                            else // Otherwise, simply calcualte costs as normal.
                            {
                                neighbor.gCost = bestRegion.gCost + bestRegion.portals[i].Cost;
                            }
                        }
                        else
                        {
                            neighbor.gCost = bestRegion.gCost + bestRegion.portals[i].Cost;
                        }

                        neighbor.hCost = Mathf.Abs(Vector3.Distance(neighbor.centerPosition, goal.centerPosition)) * heuristicWeight;
                        neighbor.fCost = neighbor.gCost + neighbor.hCost;
                        neighbor.parent = bestRegion;
                        neighbor.portalOfParent = bestRegion.portals[i];

                        regionOpenList.Enqueue(neighbor, neighbor.fCost);

                        neighbor.SearchStatus = SearchStatus.OpenList;
                    }
                    else if (neighbor.SearchStatus == SearchStatus.OpenList)
                    {
                        float gCost = bestRegion.gCost + bestRegion.portals[i].Cost;
                        float fCost = gCost + neighbor.hCost;

                        if (fCost < neighbor.fCost)
                        {
                            neighbor.gCost = gCost;
                            neighbor.fCost = fCost;
                            neighbor.parent = bestRegion;
                            neighbor.portalOfParent = bestRegion.portals[i];
                        }
                    }
                    else if (neighbor.SearchStatus == SearchStatus.ClosedList)
                    {
                        continue;
                    }
                }
                closedList.Add(bestRegion);
                bestRegion.SearchStatus = SearchStatus.ClosedList;
            }
        }

        return null;
    }

    private int heuristicWeightIndex = 1;
    private float[] heuristicWeights = new float[] { 0f, 1f, 2f };

    public void NextHeuristicWeight()
    {
        heuristicWeightIndex++;
        if (heuristicWeightIndex >= heuristicWeights.Length)
        {
            heuristicWeightIndex = 0;
        }

        AdjustHeuristicWeight(heuristicWeights[heuristicWeightIndex]);
    }
    public void PreviousHeuristicWeight()
    {
        heuristicWeightIndex--;
        if (heuristicWeightIndex < 0)
        {
            heuristicWeightIndex = heuristicWeights.Length - 1;
        }

        AdjustHeuristicWeight(heuristicWeights[heuristicWeightIndex]);
    }

    /// <summary> 0=0%, 1=100%, 2=200% </summary>
    public void SelectHeuristicWeight(int index)
    {
        heuristicWeightIndex = index;
        AdjustHeuristicWeight(heuristicWeights[index]);
    }

    void AdjustHeuristicWeight(float weight)
    {
        heuristicWeight = Mathf.Clamp(weight, 0f, 2f);
    }

    #region PathSmoothing
    const float v07 = -0.0703125f;
    const float v86 = 0.8671875f;
    const float v22 = 0.2265625f;
    const float v02 = -0.0234375f;
    const float v06 = -0.0625f;
    const float v56 = 0.5625f;

    /// <summary> Catmull-Rom Spline </summary>
    Path SmoothPath(Path path)
    {
        List<PathPoint> pathPoints = path.points;
        
        if (pathPoints.Count < 3)
        {
            // Path does not contain enough point to be smoothed.
            return path;
        }

        Path smoothPath = new Path();

        Vector3 u025;
        Vector3 u050;
        Vector3 u075;

        // First section of path
        smoothPath.AddPoint(pathPoints[0]);

        u025 = pathPoints[0].position * v07 + pathPoints[0].position * v86 + pathPoints[1].position * v22 + pathPoints[2].position * v02;
        smoothPath.AddPoint(new PathPoint(PathPointType.Curve, null, null, u025));

        u050 = pathPoints[0].position * v06 + pathPoints[0].position * v56 + pathPoints[1].position * v56 + pathPoints[2].position * v06;
        smoothPath.AddPoint(new PathPoint(PathPointType.Curve, null, null, u050));

        u075 = pathPoints[0].position * v02 + pathPoints[0].position * v22 + pathPoints[1].position * v86 + pathPoints[2].position * v07;
        smoothPath.AddPoint(new PathPoint(PathPointType.Curve, null, null, u075));


        // All other sections except last
        for (int i = 1; i < pathPoints.Count - 2; i++)
        {
            smoothPath.AddPoint(pathPoints[i]);

            u025 = pathPoints[i - 1].position * v07 + pathPoints[i].position * v86 + pathPoints[i + 1].position * v22 + pathPoints[i + 2].position * v02;
            smoothPath.AddPoint(new PathPoint(PathPointType.Curve, null, null, u025));

            u050 = pathPoints[i - 1].position * v06 + pathPoints[i].position * v56 + pathPoints[i + 1].position * v56 + pathPoints[i + 2].position * v06;
            smoothPath.AddPoint(new PathPoint(PathPointType.Curve, null, null, u050));

            u075 = pathPoints[i - 1].position * v02 + pathPoints[i].position * v22 + pathPoints[i + 1].position * v86 + pathPoints[i + 2].position * v07;
            smoothPath.AddPoint(new PathPoint(PathPointType.Curve, null, null, u075));
        }

        // Last secion of path
        int lastIndex = pathPoints.Count - 1;
        smoothPath.AddPoint(pathPoints[lastIndex - 1]);

        u025 = pathPoints[lastIndex - 2].position * v07 + pathPoints[lastIndex - 1].position * v86 + pathPoints[lastIndex].position * v22 + pathPoints[lastIndex].position * v02;
        smoothPath.AddPoint(new PathPoint(PathPointType.Curve, null, null, u025));

        u050 = pathPoints[lastIndex - 2].position * v06 + pathPoints[lastIndex - 1].position * v56 + pathPoints[lastIndex].position * v56 + pathPoints[lastIndex].position * v06;
        smoothPath.AddPoint(new PathPoint(PathPointType.Curve, null, null, u050));

        u075 = pathPoints[lastIndex - 2].position * v02 + pathPoints[lastIndex - 1].position * v22 + pathPoints[lastIndex].position * v86 + pathPoints[lastIndex].position * v07;
        smoothPath.AddPoint(new PathPoint(PathPointType.Curve, null, null, u075));

        smoothPath.AddPoint(pathPoints[lastIndex]);

        return smoothPath;
    }
    #endregion

    Path RemoveCollinearPoints(Path path)
    {
        List<PathPoint> pathPoints = path.points;

        if (pathPoints.Count < 3)
        {
            // There are not enough points!
            return path;
        }

        Path reducedPath = new Path();

        float distanceAB;
        float distanceBC;
        float distanceAC;

        // First point, will always be added
        reducedPath.AddPoint(pathPoints[0]);

        int indexA = 0;

        for (int indexB = 1; indexB < pathPoints.Count - 1; indexB++)
        {
            distanceAB = Vector3.Distance(pathPoints[indexB].position, pathPoints[indexA].position);
            distanceBC = Vector3.Distance(pathPoints[indexB + 1].position, pathPoints[indexB].position);
            distanceAC = Vector3.Distance(pathPoints[indexB + 1].position, pathPoints[indexA].position);

            if (Mathf.Approximately(distanceAB + distanceBC, distanceAC)) //(distanceAB + distanceBC == distanceAC) 
            {
                // Collinear, do not add point at indexB
                continue;
            }
            else
            {
                reducedPath.AddPoint(pathPoints[indexB]);
                indexA = indexB;
            }
        }

        // Last point, will always be added
        reducedPath.AddPoint(pathPoints[pathPoints.Count - 1]);

        return reducedPath;
    }
}
