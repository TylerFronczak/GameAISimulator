//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References:
//  1. "ERIT - A Collection of Effecient and Reliable Intersection Tests" by Martin Held
//  2. https://www.youtube.com/watch?v=GnvYEbaSBoY
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;

public class SphereLineSegmentCollisionTest : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;

    [SerializeField] float lineLength;

    [SerializeField] List<GameObject> spheres;

	void Start ()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(1, transform.forward * lineLength);

        //steeringForceLineRenderer.positionCount = 2;

        //ComputeNormalProjection(new Vector3(1, 0, 3), new Vector3(3, 0, 2));
    }

    //[SerializeField] LineRenderer steeringForceLineRenderer;

    /// <summary> Assumes the line's starting position is not in the given sphere. 
    /// SqrMagnitude replaces distance for effeciency.</summary>
    Intersection GetLineSphereIntersectionVector(GameObject sphere, Vector3 positionA, Vector3 positionB)
    {
        Vector3 positionP;
        float sphereRadius;
        positionP = sphere.transform.position;
        sphereRadius = sphere.transform.lossyScale.x / 2;

        Vector3 vectorAB = (positionB - positionA);
        Vector3 vectorAP = (positionP - positionA);

        // The line's direction is away from the sphere if.
        if (Vector3.Dot(vectorAB, vectorAP) < 0)
        {
            return null;
        }
        else
        {
            float magnitudeAB = vectorAB.magnitude;

            // The line segment is not long enough to reach the sphere if.
            if (vectorAP.magnitude - sphereRadius > magnitudeAB)
            {
                //Debug.Log("NotLongEnough");
                return null;
            }

            // C is the normal projection of P on the line
            //Vector3 normalProjectionC = Vector3.Dot()
            //Vector3 projectionPointC = (Vector3.Dot(positionP, positionB) / Vector3.Dot(positionB, positionB)) * positionB;
            //Vector3 projectionPointC = ComputeNormalProjection(positionA, positionP, positionA, positionB);
            Vector3 projectionPointC = ComputeNormalProjection(vectorAP, vectorAB);

            Vector3 vectorAC = projectionPointC - positionA;
            Vector3 vectorPC = projectionPointC - positionP;

            // C lies between A and B if
            if (vectorAC.sqrMagnitude < vectorAB.sqrMagnitude)
            {
                // Assuming C lies between A and B, PC intersects the line if
                if (vectorPC.sqrMagnitude <= sphereRadius * sphereRadius)
                {
                    return new Intersection(positionP, positionP + vectorPC.normalized * (sphereRadius * 2));
                }
            }
            else
            {
                Vector3 vectorBP = positionP - positionB;
                // The end of the line is in the sphere if
                if (vectorBP.sqrMagnitude < sphereRadius * sphereRadius)
                {
                    return new Intersection(positionP, positionP + vectorPC.normalized * (sphereRadius * 2));
                }
            }
        }
        return null;
    }

    /// <summary> Assumes the line's starting position is not in the given sphere. 
    /// SqrMagnitude replaces distance for effeciency.</summary>
    bool IsLineIntersecting(GameObject sphere, Vector3 positionA, Vector3 positionB)
    {
        Vector3 positionP;
        float sphereRadius;
        positionP = sphere.transform.position;
        sphereRadius = sphere.transform.lossyScale.x / 2;

        Vector3 vectorAB = (positionB - positionA);
        Vector3 vectorAP = (positionP - positionA);

        // The line's direction is away from the sphere if.
        if (Vector3.Dot(vectorAB, vectorAP) < 0) 
        {
            return false;
        }
        else
        {
            float magnitudeAB = vectorAB.magnitude;

            // The line segment is not long enough to reach the sphere if.
            if (vectorAP.magnitude - sphereRadius > magnitudeAB)
            {
                Debug.Log("NotLongEnough");
                return false;
            }

            // C is the normal projection of P on the line
            //Vector3 normalProjectionC = Vector3.Dot()
            Vector3 projectionPointC = (Vector3.Dot(positionP, positionB) / Vector3.Dot(positionB, positionB)) * positionB;
            //Vector3 projectionPointC = ComputeNormalProjection(positionA, positionP, positionA, positionB);
            Vector3 vectorAC = projectionPointC - positionA;
            Vector3 vectorPC = projectionPointC - positionP;

            // C lies between A and B if
            if (vectorAC.sqrMagnitude < vectorAB.sqrMagnitude)
            {
                // Assuming C lies between A and B, PC intersects the line if
                if (vectorPC.sqrMagnitude <= sphereRadius * sphereRadius)
                {
                    return true;
                }
            }
            else
            {
                Vector3 vectorBP = positionP - positionB;
                // The end of the line is in the sphere if
                if (vectorBP.sqrMagnitude < sphereRadius * sphereRadius)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary> Assumes the line's starting position is not in the given sphere. 
    /// SqrMagnitude replaces distance for effeciency.</summary>
    bool IsCylinderIntersecting(GameObject sphere, float cylinderRadius, Vector3 positionA, Vector3 positionB)
    {
        Vector3 positionP;
        float sphereRadius;
        positionP = sphere.transform.position;
        sphereRadius = sphere.transform.lossyScale.x / 2;

        Vector3 vectorAB = (positionB - positionA);
        Vector3 vectorAP = (positionP - positionA);

        // The line's direction is away from the sphere if.
        if (Vector3.Dot(vectorAB, vectorAP) < 0)
        {
            return false;
        }
        else
        {
            float magnitudeAB = vectorAB.magnitude;

            // The line segment is not long enough to reach the sphere if.
            if (vectorAP.magnitude - sphereRadius > magnitudeAB)
            {
                Debug.Log("NotLongEnough");
                return false;
            }

            // C is the normal projection of P on the line
            //Vector3 normalProjectionC = Vector3.Dot()
            Vector3 projectionPointC = (Vector3.Dot(positionP, positionB) / Vector3.Dot(positionB, positionB)) * positionB;
            //Vector3 projectionPointC = ComputeNormalProjection(positionA, positionP, positionA, positionB);
            Vector3 vectorAC = projectionPointC - positionA;
            Vector3 vectorPC = projectionPointC - positionP;

            // C lies between A and B if
            if (vectorAC.sqrMagnitude < vectorAB.sqrMagnitude)
            {
                // Assuming C lies between A and B, PC intersects the line if
                if (vectorPC.magnitude <= sphereRadius + cylinderRadius)
                {
                    return true;
                }
            }
            else
            {
                Vector3 vectorBP = positionP - positionB;
                // The end of the capsule is in the sphere if
                if (vectorBP.magnitude < sphereRadius + cylinderRadius)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public Vector3 GetSteeringOffset()
    {
        for (int i = 0; i < spheres.Count; i++)
        {
            // if (IsCylinderIntersecting(spheres[i], 0.5f, transform.position, transform.position + transform.forward * lineLength))
            //if (IsLineIntersecting(spheres[i], transform.position, transform.position + transform.forward * lineLength))
            Intersection intersection = GetLineSphereIntersectionVector(spheres[i], transform.position, transform.position + transform.forward * lineLength);
            if (intersection != null)
            {
                //steeringForceLineRenderer.SetPosition(0, intersection.positionP);
                //steeringForceLineRenderer.SetPosition(1, intersection.positionP + intersection.steeringOffset);
                //Debug.Log("IsIntersecting: " + spheres[i].name);
                return intersection.steeringOffset;
            }
        }

        return Vector3.zero;
    }

    //[SerializeField] LineRenderer lineX;
    //[SerializeField] LineRenderer lineV;
    //[SerializeField] GameObject projectionMarker;

    /// <summary>
    /// Projection point of vector X onto vector Y
    /// </summary>
    Vector3 ComputeNormalProjection(Vector3 vectorX, Vector3 vectorV)
    {
        Vector3 projectionPoint = transform.position + (Vector3.Dot(vectorX, vectorV) / Vector3.Dot(vectorV, vectorV)) * vectorV;

        //projectionMarker.transform.position = projectionPoint;
        //lineX.positionCount = 2;
        //lineX.SetPosition(0, transform.position);
        //lineX.SetPosition(1, transform.position + vectorX);
        //lineV.positionCount = 2;
        //lineV.SetPosition(0, transform.position);
        //lineV.SetPosition(1, transform.position + vectorV);

        return projectionPoint;
    }
}

public class Intersection
{
    /// <summary> WARNING: Poor metric for large obstacles. </summary>
    public Vector3 positionP; 
    public Vector3 steeringOffset;

    public Intersection(Vector3 positionP, Vector3 steeringOffset)
    {
        this.positionP = positionP;
        this.steeringOffset = steeringOffset;
    }
}
