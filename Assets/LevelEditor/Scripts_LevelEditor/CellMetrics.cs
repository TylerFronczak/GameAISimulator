//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. http://catlikecoding.com/unity/tutorials/
//*************************************************************************************************

using UnityEngine;

/// <summary>
/// Contains information that is applicable to every cell.
/// </summary>
public static class CellMetrics
{
    /// <summary> Distance from center of cell to any of its faces. </summary>
    public const float radius = 0.5f;

    /// <summary> Distance between the centers of adjacent cells. (offset=radius*2) </summary>
    public const float offset = 1f;
    
    /// <summary> Relative position offsets from a cell's corners to its center. </summary>
    public static Vector3[] corners =
    {
        //TopLeft
        new Vector3(-radius, 0f, radius),

        //TopRight
        new Vector3(radius, 0, radius),

        //BottomRight
        new Vector3(radius, 0, -radius),

        //BottomLeft
        new Vector3(-radius, 0, -radius),

        //TopLeft- Added again for constructing final triangle, rather than checking index bounds.
        new Vector3(-radius, 0f, radius)
    };
}
