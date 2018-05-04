//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. https://answers.unity.com/questions/722531/getting-perpendicular-direction-vector-from-surfac.html
//  2. https://forum.unity.com/threads/rotating-a-vector-by-an-eular-angle.18485/
//*************************************************************************************************

using UnityEngine;
using GameAISimulator.Enums;

public class CollisionTester : MonoBehaviour
{
    [SerializeField] LineRenderer leftLineRenderer;
    Transform leftLineTransform;

    [SerializeField] LineRenderer centerLineRenderer;
    Transform centerLineTransform;

    [SerializeField] LineRenderer rightLineRenderer;
    Transform rightLineTransform;

    [SerializeField] float lineLength;

    [SerializeField] LayerMask collisionLayer;

    [SerializeField] Material noCollisionMaterial;
    [SerializeField] Material collisionMaterial;


    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        leftLineTransform = leftLineRenderer.gameObject.transform;
        centerLineTransform = centerLineRenderer.gameObject.transform;
        rightLineTransform = rightLineRenderer.gameObject.transform;

        leftLineRenderer.positionCount = 2;
        centerLineRenderer.positionCount = 2;
        rightLineRenderer.positionCount = 2;

        leftLineRenderer.SetPosition(1, leftLineTransform.forward * lineLength);
        centerLineRenderer.SetPosition(1, centerLineTransform.forward * lineLength);
        rightLineRenderer.SetPosition(1, rightLineTransform.forward * lineLength);
    }

    private void Update()
    {
        DetectCollision();
    }

    public TurningStatus DetectCollision()
    {
        TurningStatus turningStatus = TurningStatus.None;

        RaycastHit centerHit;
        if (Physics.Raycast(centerLineTransform.position, centerLineTransform.forward, out centerHit, lineLength, collisionLayer))
        {
            centerLineRenderer.material = collisionMaterial;
        }
        else
        {
            centerLineRenderer.material = noCollisionMaterial;
        }

        RaycastHit leftHit;
        if (Physics.Raycast(leftLineTransform.position, leftLineTransform.forward, out leftHit, lineLength, collisionLayer))
        {
            leftLineRenderer.material = collisionMaterial;
        }
        else
        {
            leftLineRenderer.material = noCollisionMaterial;
        }

        RaycastHit rightHit;
        if (Physics.Raycast(rightLineTransform.position, rightLineTransform.forward, out rightHit, lineLength, collisionLayer))
        {
            rightLineRenderer.material = collisionMaterial;
        }
        else
        {
            rightLineRenderer.material = noCollisionMaterial;
        }

        if (leftHit.collider)
        {
            turningStatus = TurningStatus.CW;
        }
        else if (rightHit.collider)
        {
            turningStatus = TurningStatus.CCW;
        }
        else
        {
            turningStatus = TurningStatus.None;
        }

        return turningStatus;
    }

    public Vector3 ComputeSurfaceParallel(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    public Vector3 DetermineClosestCollisionObjectCenter(RaycastHit[] hits)
    {
        RaycastHit closestHit = hits[0];
        float shortestDistance = Mathf.Infinity;

        for (int i = 0; i < hits.Length; i++)
        {
            float distance = hits[i].distance;
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestHit = hits[i];
            }
        }

        return closestHit.collider.transform.position;
    }
}
