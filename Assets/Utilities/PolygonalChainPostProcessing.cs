//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;

public class PolygonalChainPostProcessing : MonoBehaviour
{
    [SerializeField] List<Vector3> originalChain;
    List<Vector3> postProcessedChain;

    const float v07 = -0.0703125f;
    const float v86 = 0.8671875f;
    const float v22 = 0.2265625f;
    const float v02 = -0.0234375f;
    const float v06 = -0.0625f;
    const float v56 = 0.5625f;

    /// <summary> Returns a smoothed series of points using the Catmull-Rom spline algorithm. </summary>
    List<Vector3> SmoothChain(List<Vector3> points)
    {
        // Must have at least 3 points.
        if (points.Count < 3)
        {
            return points;
        }

        List<Vector3> smoothedChain = new List<Vector3>();

        Vector3 u025;
        Vector3 u050;
        Vector3 u075;

        // First line segement
        smoothedChain.Add(points[0]);

        u025 = points[0] * v07 + points[0] * v86 + points[1] * v22 + points[2] * v02;
        smoothedChain.Add(u025);

        u050 = points[0] * v06 + points[0] * v56 + points[1] * v56 + points[2] * v06;
        smoothedChain.Add(u050);

        u075 = points[0] * v02 + points[0] * v22 + points[1] * v86 + points[2] * v07;
        smoothedChain.Add(u075);


        // All other line segements except last
        for (int i = 1; i < points.Count - 2; i++)
        {
            smoothedChain.Add(points[i]);

            u025 = points[i-1] * v07 + points[i] * v86 + points[i+1] * v22 + points[i+2] * v02;
            smoothedChain.Add(u025);

            u050 = points[i-1] * v06 + points[i] * v56 + points[i+1] * v56 + points[i+2] * v06;
            smoothedChain.Add(u050);

            u075 = points[i-1] * v02 + points[i] * v22 + points[i+1] * v86 + points[i+2] * v07;
            smoothedChain.Add(u075);
        }

        // Last line segment
        int lastIndex = points.Count - 1;
        smoothedChain.Add(points[lastIndex-1]);

        u025 = points[lastIndex-2] * v07 + points[lastIndex-1] * v86 + points[lastIndex] * v22 + points[lastIndex] * v02;
        smoothedChain.Add(u025);

        u050 = points[lastIndex-2] * v06 + points[lastIndex-1] * v56 + points[lastIndex] * v56 + points[lastIndex] * v06;
        smoothedChain.Add(u050);

        u075 = points[lastIndex-2] * v02 + points[lastIndex-1] * v22 + points[lastIndex] * v86 + points[lastIndex] * v07;
        smoothedChain.Add(u075);

        smoothedChain.Add(points[lastIndex]);

        return smoothedChain;
    }

    List<Vector3> RemoveCollinearPoints(List<Vector3> points)
    {
        // Must have at least 3 points.
        if (points.Count < 3)
        {
            return points;
        }

        List<Vector3> reducedPoints = new List<Vector3>();

        float distanceAB;
        float distanceBC;
        float distanceAC;
        
        // First point, will always be added.
        reducedPoints.Add(points[0]);

        int indexA = 0;

        for (int indexB = 1; indexB < points.Count - 1; indexB++)
        {
            distanceAB = Vector3.Distance(points[indexB], points[indexA]);
            distanceBC = Vector3.Distance(points[indexB + 1], points[indexB]);
            distanceAC = Vector3.Distance(points[indexB + 1], points[indexA]);

            if (Mathf.Approximately(distanceAB + distanceBC, distanceAC)) //(distanceAB + distanceBC == distanceAC) 
            {
                // Collinear, do not add point at indexB.
                continue;
            }
            else
            {
                reducedPoints.Add(points[indexB]);
                indexA = indexB;
            }
        }

        // Last point, will always be added.
        reducedPoints.Add(points[points.Count - 1]);

        return reducedPoints;
    }

    private void OnDrawGizmos()
    {
        if (isSplineCreated)
        {
            for (int i = 0; i < postProcessedChain.Count; i++)
            {
                Gizmos.DrawSphere(postProcessedChain[i], 0.1f);
            }
        }
    }

    #region Testing
    [SerializeField] int testInterations;
    bool isSplineCreated;

    private void Test()
    {
        float startTime = Time.realtimeSinceStartup;
        for (int i = 0; i < testInterations; i++)
        {
            postProcessedChain = SmoothChain(originalChain);
            postProcessedChain = RemoveCollinearPoints(postProcessedChain);
        }
        float endTime = Time.realtimeSinceStartup;
        Debug.Log("Time to smooth path of length " + originalChain.Count + " (" + testInterations + "x): " + ((endTime - startTime) / testInterations));
        isSplineCreated = true;
    }
    #endregion
}
