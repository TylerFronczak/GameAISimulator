//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using GameAISimulator;
using GameAISimulator.Enums;
using BehaviorTreeSystem.Enums;

public class Wander : AgentAction
{
    float timeWandering;

    public Wander(Agent agentPerformingAction) : base(agentPerformingAction)
    {

    }

    protected override void OnInitialize()
    {
        agent.movementMode = MovementMode.Steering;
        agent.simpleVehicleModel.ToggleWander(true);
        agent.simpleVehicleModel.isAvoidingCollision = true;
        timeWandering = 0f;
    }

    protected override BehaviorStatus OnUpdate()
    {
        //Debug.Log(string.Format("{0}: Wander.", agent.agentName));

        if (timeWandering >= 0.1f)
        {
            return BehaviorStatus.Success;
        }
        else
        {
            // Time tracker is incremented after checking to ensure no time is registered as passing between OnItitialize() and the first OnUpdate().
            timeWandering += SimulationData.realSecondsBetweenAgentBehaviorUpdates;

            return BehaviorStatus.Running;
        }
    }

    protected override void OnTerminate(BehaviorStatus status)
    {
        agent.simpleVehicleModel.ResetSteeringBehaviors();
    }
}
