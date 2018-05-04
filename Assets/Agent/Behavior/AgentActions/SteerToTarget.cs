//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;
using GameAISimulator.Enums;
using BehaviorTreeSystem.Enums;

public class SteerToTarget : AgentAction
{
    private Transform targetTransform;

    public SteerToTarget(Agent agentPerformingAction) : base(agentPerformingAction)
    {

    }

    protected override void OnInitialize()
    {
        agent.movementMode = MovementMode.Steering;
        targetTransform = agent.target.transform;
        agent.simpleVehicleModel.SeekTo(targetTransform);
    }

    protected override BehaviorStatus OnUpdate()
    {
        //Debug.Log(string.Format("{0}: SteerToTarget.", agent.agentName));

        if (targetTransform == null)
        {
            Debug.LogWarning("Unable to steer towards null target");
            return BehaviorStatus.Failure;
        }

        if (Vector3.Distance(agent.transform.position, targetTransform.position) <= 1.1f)
        {
            return BehaviorStatus.Success;
        }

        return BehaviorStatus.Running;
    }

    protected override void OnTerminate(BehaviorStatus status)
    {
        agent.simpleVehicleModel.ResetSteeringBehaviors();
    }
}
