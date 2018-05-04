//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References:
//  1. "Steering Behaviors For Autonomous Characters" by Craig W.Reynolds
//  2. https://docs.unity3d.com/Manual/QuaternionAndEulerRotationsInUnity.html
//  3. https://www.khanacademy.org/math/linear-algebra/vectors-and-spaces/dot-cross-products/v/proving-vector-dot-product-properties
//  4. "ERIT - A Collection of Effecient and Reliable Intersection Tests" by Martin Held
//  5. https://www.youtube.com/watch?v=72QUktwvBNA&list=PLYyd_HRy1Kt9vqvaffMZEkv_RQoZP0794&t=3s&index=11
//  6. https://www.youtube.com/watch?v=_ENEsV_kNx8&list=PLRqwX-V7Uu6YHt0dtyf4uiw8tKOxQLvlW&index=5
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using GameAISimulator.Enums;

public class SimpleVehicleModel : MonoBehaviour
{
    Transform cachedTransform;

    private float mass = 1; //{ get { return agentData.PhyicsMass; } }
    Vector3 velocity;
    public float maxSteeringScalar;
    public float maxSpeed;

    [SerializeField] LineRenderer velocityLineRenderer;
    [SerializeField] LineRenderer steeringForceLineRenderer;

    public bool isVisualizingVelocity;
    public bool isVisualizingSteeringVector;

    Vector3 currentSteeringForce;
    Vector3 acceleration;

    Vector3 steeringVector;

    public Transform targetTransform;
    public SimpleVehicleModel movingTarget;

    public bool isSeeking;
    public bool isFleeing;
    public bool isPursuing;
    public bool isEvading;
    public bool isArriving;
    public bool IsWandering { get; private set; }
    public bool isAvoidingCollision;
    public bool isSeperating;
    public bool isLeaderFollowing;

    [Header("Pursuit")]
    [SerializeField] GameObject pursuitMarker;

    [Header("Arrival")]
    [SerializeField] float arrivalSlowingDistance = 2f;

    [Header("Evasion")]
    [SerializeField] GameObject evasionMarker;

    [Header("Wander")]
    [SerializeField] GameObject wanderLargeCircle;
    [SerializeField] GameObject wanderSmallCircle;
    Vector3 wanderSmallCircleRelativePosition;
    bool isWanderInitialized;

    // Center must be first in list
    [SerializeField] List<SphereLineSegmentCollisionTest> collisionTesters;
    bool isInCollisionDanger;

    float safeRadius;
    [SerializeField] float seperationStrength;

    public SimpleVehicleModel leader;

    [Header("PathFollowing")]
    public bool isPathFollowing;
    [SerializeField] Transform predictedLocationMarker;
    [SerializeField] Transform normalPointMarker;
    [SerializeField] Transform futurePathTargetMarker;
    public SimplePath simplePath;
    [SerializeField] float predictionDistance;

    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Material defualtMaterial;
    [SerializeField] Material highlightMaterial;
    [SerializeField] Material aggressiveMaterial;

    public void BumpForward()
    {
        steeringVector = transform.forward;
    }

    public void ChangeMaterialDefualt()
    {
        meshRenderer.material = defualtMaterial;
    }

    public void ChangeMaterialHighlight()
    {
        meshRenderer.material = highlightMaterial;
    }

    public void ChangeMaterialAggressive()
    {
        meshRenderer.material = aggressiveMaterial;
    }

    public void Initialize()
    {
        cachedTransform = transform;
        steeringVector = transform.forward;
        safeRadius = transform.lossyScale.x;
        velocityLineRenderer.positionCount = 2;
        steeringForceLineRenderer.positionCount = 2;
    }

    public void SeekTo(Transform target)
    {
        isSeeking = true;
        targetTransform = target;
    }

    public void UpdateSteering()
    {
        steeringVector = transform.forward;

        if (isSeeking && targetTransform != null)
        {
            steeringVector = SteeringBehaviors.Seek(cachedTransform.position, velocity, maxSpeed, targetTransform.position);
        }

        if (isFleeing && targetTransform != null)
        {
            steeringVector = SteeringBehaviors.Flee(cachedTransform.position, velocity, maxSpeed, targetTransform.position);
        }

        if (isPursuing && movingTarget != null)
        {
            steeringVector = SteeringBehaviors.Pursuit(cachedTransform.position, velocity, maxSpeed, movingTarget.transform.position, movingTarget.velocity);
        }

        if (isEvading && movingTarget != null)
        {
            steeringVector = SteeringBehaviors.Evasion(cachedTransform.position, velocity, maxSpeed, movingTarget.transform.position, movingTarget.velocity);
        }

        if (isArriving && targetTransform != null)
        {
            steeringVector = SteeringBehaviors.Arrival(cachedTransform.position, velocity, maxSpeed, targetTransform.position, arrivalSlowingDistance);
        }

        if (IsWandering)
        {
            steeringVector = Wander(0.5f, 0.1f, false);
        }

        if (isLeaderFollowing)
        {
            Vector3 leaderFollowPoint = leader.transform.position + leader.transform.forward * -2f;
            steeringVector = SteeringBehaviors.Arrival(cachedTransform.position, velocity, maxSpeed, leaderFollowPoint, arrivalSlowingDistance);
        }

        if (isPathFollowing)
        {
            Vector3 possibleSteeringVector = PathFollowing(simplePath);

            if (possibleSteeringVector != Vector3.zero)
            {
                steeringVector = possibleSteeringVector;
            }
        }

        if (isSeperating)
        {
            Vector3 repulsionLocation = cachedTransform.position + Seperation();

            if (repulsionLocation != cachedTransform.position)
            {
                Vector3 desiredVelocity = (repulsionLocation - cachedTransform.position).normalized * maxSpeed;

                steeringVector = desiredVelocity - velocity;
            }
        }

        if (isAvoidingCollision)
        {
            Vector3 collisionSteeringVector = CollisionAvoidance();

            if (collisionSteeringVector != velocity)
            {
                if (collisionSteeringVector == transform.forward * maxSpeed && steeringVector != transform.forward)
                {
                    // The previously applied steering vector should not be modified.
                }
                else
                {
                    steeringVector = collisionSteeringVector;
                }

            }
        }

        if (velocity != Vector3.zero) //Check is to prevent the log message "Look rotation viewing vector is zero"
        {
            transform.rotation = Quaternion.LookRotation(velocity);
        }

        UpdateMovement();
    }

    void UpdateMovement()
    {
        currentSteeringForce = Vector3.ClampMagnitude(steeringVector, maxSteeringScalar);
        acceleration = currentSteeringForce / mass;
        velocity = Vector3.ClampMagnitude(velocity + acceleration, maxSpeed);
        cachedTransform.position += velocity * Time.fixedDeltaTime;

        if (isVisualizingVelocity)
        {
            velocityLineRenderer.SetPosition(0, cachedTransform.position);
            velocityLineRenderer.SetPosition(1, cachedTransform.position + velocity);
        }

        if (isVisualizingSteeringVector)
        {
            steeringForceLineRenderer.SetPosition(0, cachedTransform.position);
            steeringForceLineRenderer.SetPosition(1, cachedTransform.position + steeringVector);
        }
    }

    public void ToggleWander(bool isEnabled)
    {
        IsWandering = isEnabled;
        wanderLargeCircle.SetActive(isEnabled);
        wanderSmallCircle.SetActive(isEnabled);
    }

    public void TogglePursuit(bool isEnabled)
    {
        isPursuing = isEnabled;
        pursuitMarker.SetActive(isEnabled);
    }

    public void ToggleEvasion(bool isEnabled)
    {
        isEvading = isEnabled;
        evasionMarker.SetActive(isEnabled);
    }

    public void TogglePathFollowing(bool isEnabled)
    {
        isPathFollowing = isEnabled;
        predictedLocationMarker.gameObject.SetActive(isEnabled);
        normalPointMarker.gameObject.SetActive(isEnabled);
        futurePathTargetMarker.gameObject.SetActive(isEnabled);
    }

    public void ResetSteeringBehaviors()
    {
        if (isLeaderFollowing)
        {
            ChangeMaterialDefualt();
        }

        isSeeking = false;
        isFleeing = false;

        TogglePursuit(false);
        ToggleEvasion(false);

        isArriving = false;

        ToggleWander(false);

        isAvoidingCollision = false;
        isSeperating = false;
        isLeaderFollowing = false;

        TogglePathFollowing(false);
    }

    public void ToggleVisualization_Velocity()
    {
        isVisualizingVelocity = !isVisualizingVelocity;
        velocityLineRenderer.gameObject.SetActive(isVisualizingVelocity);
    }

    public void ToggleVisualization_Velocity(bool isEnabled)
    {
        isVisualizingVelocity = isEnabled;
        velocityLineRenderer.gameObject.SetActive(isEnabled);
    }

    public void ToggleVisualization_Steering()
    {
        isVisualizingSteeringVector = !isVisualizingSteeringVector;
        steeringForceLineRenderer.gameObject.SetActive(isVisualizingSteeringVector);
    }

    public void ToggleVisualization_Steering(bool isEnabled)
    {
        isVisualizingSteeringVector = isEnabled;
        steeringForceLineRenderer.gameObject.SetActive(isEnabled);
    }

    bool isVisualizingCurrentBehavior;
    public bool IsVisualizingCurrentBehavior
    {
        set
        {
            isVisualizingCurrentBehavior = value;
            wanderLargeCircle.GetComponent<SpriteRenderer>().enabled = value;
            wanderSmallCircle.GetComponent<SpriteRenderer>().enabled = value;
        }
    }

    /// <summary> Large circle determines max wander strength and small circle determines the magnitude of each random displacement. </summary>
    Vector3 Wander(float largeCircleRadius, float smallCircleRadius, bool isMovingY)
    {
        if (!isWanderInitialized)
        {
            isWanderInitialized = true;
            wanderLargeCircle.transform.position = cachedTransform.position + cachedTransform.forward * (largeCircleRadius + 1);
            wanderSmallCircle.transform.position = wanderLargeCircle.transform.position;
        }

        Vector3 randomDirection;
        if (!isMovingY) {
            randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        } else {
            randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1, 1)).normalized;
        }

        wanderSmallCircle.transform.localPosition += randomDirection * smallCircleRadius;

        // If small circle position is outside the allowed area, clamp to edge of large circle.
        if (Vector3.Distance(wanderSmallCircle.transform.position, wanderLargeCircle.transform.position) > largeCircleRadius)
        {
            Vector3 directionFromLargeToSmall = (wanderSmallCircle.transform.position - wanderLargeCircle.transform.position).normalized;
            wanderSmallCircle.transform.position = wanderLargeCircle.transform.position + directionFromLargeToSmall * largeCircleRadius;
        }

        Vector3 desiredVelocity = (wanderSmallCircle.transform.position - cachedTransform.position).normalized * maxSpeed;
        return desiredVelocity - velocity;
    }

    [SerializeField] CollisionTester collisionTester;

    Vector3 CollisionAvoidance()
    {
        //Vector3 offsetPosition;

        //for (int i = 0; i < collisionTesters.Count; i++)
        //{
        //    offsetPosition = collisionTesters[i].GetSteeringOffset();

        //    if (offsetPosition != Vector3.zero)
        //    {
        //        Vector3 desiredVelocity = (offsetPosition - transform.position).normalized * maxSpeed;
        //        return desiredVelocity - velocity;
        //    }
        //}

        TurningStatus turningStatus = collisionTester.DetectCollision();

        if (turningStatus == TurningStatus.CW)
        {
            return transform.right * maxSpeed;
        }
        else if (turningStatus == TurningStatus.CCW)
        {
            return -transform.right * maxSpeed;
        }
        else
        {
            return transform.forward * maxSpeed;
        }
    }

    Vector3 Seperation()
    {
        List<SimpleVehicleModel> boids = BoidManager.Instance.boids;

        Vector3 repulsionVectorSum = Vector3.zero;
        
        for (int i = 0; i < boids.Count; i++)
        {
            if (boids[i] == this)
            {
                continue;
            }

            Vector3 vectorFromOther = cachedTransform.position - boids[i].transform.position;
            float distance = vectorFromOther.magnitude;
            float safeDistance = safeRadius + boids[i].safeRadius;

            if (distance < safeDistance)
            {
                repulsionVectorSum += vectorFromOther.normalized * ((safeDistance - distance) / safeDistance);
            }
        }

        if (repulsionVectorSum.magnitude > 1.0f)
        {
            repulsionVectorSum = repulsionVectorSum.normalized;
        }

        return repulsionVectorSum *= seperationStrength;
    }

    Vector3 PathFollowing(SimplePath path)
    {
        Vector3 predictedLocation = cachedTransform.position + velocity.normalized * predictionDistance;// maxSpeed;
        predictedLocationMarker.position = predictedLocation;

        float shortestSqrMagnitude = Mathf.Infinity;
        int startIndexOfClosestLine = -1;
        Vector3 closestNormalPoint = Vector3.zero;

        // Find the closest normal point by checking against all lines in path.
        for (int i = 0; i < path.points.Count - 1; i++)
        {
            Vector3 normalPoint = ComputeClampedNormalPoint(predictedLocation, path.points[i], path.points[i+1]);

            float sqrMagnitudeAB = (path.points[i + 1] - path.points[i]).sqrMagnitude;

            // The normal cannot be between A & B if its distance to A or B exceeds the length of AB.
            if (sqrMagnitudeAB < (normalPoint - path.points[i]).sqrMagnitude)
            {
                normalPoint = path.points[i + 1];
            }
            else if (sqrMagnitudeAB < (normalPoint - path.points[i + 1]).sqrMagnitude)
            {
                normalPoint = path.points[i];
            }

            float sqrMagnitudeFromNormal = (predictedLocation - normalPoint).sqrMagnitude;
            if (sqrMagnitudeFromNormal < shortestSqrMagnitude)
            {
                shortestSqrMagnitude = sqrMagnitudeFromNormal;
                startIndexOfClosestLine = i;
                closestNormalPoint = normalPoint;
            }
        }

        // If no valid noraml could be found, redirect the boid to the start of the path.
        if (startIndexOfClosestLine == -1)
        {
            return SteeringBehaviors.Seek(cachedTransform.position, velocity, maxSpeed, path.points[0]);
        }

        normalPointMarker.position = closestNormalPoint;

        Vector3 pathDirection = path.points[startIndexOfClosestLine + 1] - path.points[startIndexOfClosestLine];
        Vector3 futurePathTarget = closestNormalPoint + pathDirection.normalized;// * predictionDistance;
        futurePathTargetMarker.position = futurePathTarget;

        if (shortestSqrMagnitude > path.radius * path.radius)
        {
            return SteeringBehaviors.Seek(cachedTransform.position, velocity, maxSpeed, futurePathTarget);
        }
        else // No steering required
        {
            return Vector3.zero;
        }
    }

    Vector3 ComputeClampedNormalPoint(Vector3 pointP, Vector3 lineStartA, Vector3 lineEndB)
    {
        Vector3 vectorAP = pointP - lineStartA;
        Vector3 vectorAB = lineEndB - lineStartA;

        vectorAB.Normalize();
        vectorAB *= Vector3.Dot(vectorAP, vectorAB);

        return lineStartA + vectorAB;
    }
}
