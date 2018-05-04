//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References:
//  1. "Steering Behaviors For Autonomous Characters" by Craig W.Reynolds
//*************************************************************************************************

using UnityEngine;

public static class SteeringBehaviors
{
    /// <summary> Returns a steering vector for radial alignment towards a target position. </summary>
    public static Vector3 Seek(Vector3 currentPosition, Vector3 velocity, float speed, Vector3 targetPosition)
    {
        Vector3 desiredVelocity = (targetPosition - currentPosition).normalized * speed;
        return desiredVelocity - velocity;
    }

    /// <summary> Returns a steering vector for radial alignment away from a target position. </summary>
    public static Vector3 Flee(Vector3 currentPosition, Vector3 velocity, float speed, Vector3 targetPosition)
    {
        Vector3 desiredVelocity = (currentPosition - targetPosition).normalized * speed;
        return desiredVelocity - velocity;
    }

    /// <summary> Returns a steering vector for attempting to intercept the target at a future location. </summary>
    public static Vector3 Pursuit(Vector3 currentPosition, Vector3 velocity, float speed, Vector3 positionOfTarget, Vector3 velocityOfTarget)
    {
        Vector3 directionFromTarget = (currentPosition - positionOfTarget).normalized;
        float normalizedDirectionDotProduct = Vector3.Dot(velocityOfTarget.normalized, directionFromTarget);

        Vector3 targetPosition;

        if (normalizedDirectionDotProduct < -0.5f) // At back of target, so attempting to cut-off would have neglible utility.
        {
            targetPosition = positionOfTarget;
        }
        else
        {
            // Dot product value is inverted because a negative result(behind) would require a greater predicted interval, 
            // whereas a positive result(front) would require a lesser prediction interval.
            // A one is added to ensure the value is never below 0, which would result in a predicted lcoation behind the target.
            float turningParameter = ((normalizedDirectionDotProduct * -1) + 1);
            float predictionInterval = Vector3.Distance(positionOfTarget, currentPosition) * turningParameter;

            // An explicit value multiplied by the magnitude reduces optimality, but maintains a more believable prediction.
            // The value represents a unit of time and should be adjusted to fit the needs of the project.
            predictionInterval = Mathf.Clamp(predictionInterval, 0, velocityOfTarget.magnitude * 5f); 

            targetPosition = positionOfTarget + (velocityOfTarget.normalized * predictionInterval);
        }

        Vector3 desiredVelocity = (targetPosition - currentPosition).normalized * speed;
        return desiredVelocity - velocity;
    }

    /// <summary> Returns a steering vector for radial alignment away from a target's future posiiton. </summary>
    public static Vector3 Evasion(Vector3 currentPosition, Vector3 velocity, float speed, Vector3 positionOfThreat, Vector3 velocityOfThreat)
    {
        float predictionInterval = velocityOfThreat.magnitude * 5f;
        predictionInterval = Mathf.Clamp(predictionInterval, 0, Vector3.Distance(positionOfThreat, currentPosition) * 0.75f);
        Vector3 predictedLocation = positionOfThreat + (velocityOfThreat.normalized * predictionInterval);

        Vector3 desiredVelocity = (currentPosition - predictedLocation).normalized * speed;
        return desiredVelocity - velocity;
    }

    /// <summary> Returns a steering vector for radial alignment towards a target position, which has its speed dampened based upon proximity. </summary>
    public static Vector3 Arrival(Vector3 currentPosition, Vector3 velocity, float speed, Vector3 targetPosition, float slowingDistance)
    {
        Vector3 vectorToTarget = targetPosition - currentPosition;
        float distanceToTarget = vectorToTarget.magnitude;
        float dampenedSpeed = speed * (distanceToTarget / slowingDistance);
        dampenedSpeed = Mathf.Min(dampenedSpeed, speed);
        Vector3 desiredVelocity = vectorToTarget * (dampenedSpeed / distanceToTarget);
        return desiredVelocity - velocity;
    }

    // Important: Should include seperation and lateral steering away from a zone in front of the leader.
    /// <summary> Returns a steering vector for radial alignment towards a position behind the leader, based upon Arrival(). </summary>
    public static Vector3 LeaderFollowing(Vector3 currentPosition, Vector3 velocity, float speed, Transform leaderTransform, float slowingDistance)
    {
        Vector3 leaderFollowPoint = leaderTransform.position + leaderTransform.forward * -2f;
        return Arrival(currentPosition, velocity, speed, leaderFollowPoint, slowingDistance);
    }
}
