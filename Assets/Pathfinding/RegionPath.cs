//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using GameAISimulator.Enums;

public class RegionPath
{
    public List<RegionPathPoint> points = new List<RegionPathPoint>();

    bool isInitialized;

    public void AddPoint(RegionPathPoint point)
    {
        points.Add(point);
    }
}

public struct RegionPathPoint
{
    public RegionPathPointType regionPathPointType;
    public Region region;
    public Portal portal;
    public Vector3 position;

    public RegionPathPoint(RegionPathPointType regionPathPointType, Region region, Portal portal, Vector3 position)
    {
        this.regionPathPointType = regionPathPointType;
        this.region = region;
        this.portal = portal;
        this.position = position;
    }
}
