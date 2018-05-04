//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using GameAISimulator.Enums;

public class Path
{
    public List<PathPoint> points = new List<PathPoint>();

    bool isInitialized;

    public void AddPoint(PathPoint point)
    {
        if (point.pathPointType == PathPointType.Edge)
        {
            if (point.edge.EdgeType == EdgeType.SlopeConnection || point.edge.EdgeType == EdgeType.Elevated)
            {
                points.Add(point);
            }
        }
        else
        {
            points.Add(point);
        }
    }
}

public struct PathPoint
{
    public PathPointType pathPointType;
    public Node node;
    public Edge edge;
    public Vector3 position;

    public PathPoint(PathPointType pathPointType, Node node, Edge edge, Vector3 position)
    {
        this.pathPointType = pathPointType;
        this.node = node;
        this.edge = edge;
        this.position = position;
    }
}
