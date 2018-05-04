//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;
using BehaviorTreeSystem.Enums;
using GameAISimulator;

public class Eat : AgentAction
{
    Food targetFood;
    float timeEating;

    bool hasEatenFood;

    public Eat(Agent agentPerformingAction) : base(agentPerformingAction)
    {

    }

    protected override void OnInitialize()
    {
        if (agent.target == null)
        {
            targetFood = null;
            Debug.LogError("Must assign target before attempting to eat.");
        }
        else
        {
            targetFood = agent.target.GetComponentInParent<Food>();
            hasEatenFood = false;
            timeEating = 0f;
        }
    }

    protected override BehaviorStatus OnUpdate()
    {
        //Debug.Log(string.Format("{0}: Eat.", agent.agentName));

        if (targetFood == null || targetFood.isDepleted)
        {
            if (hasEatenFood)
            {
                return BehaviorStatus.Success;
            }
            else //Didn't get to eat any food before it was gone.
            {
                return BehaviorStatus.Failure;
            }
        }
        else
        {
            float foodEnergy = targetFood.DrainEnergy(agent.stats.metabolicRate, 1f, SimulationData.simDaysBetweenBehaviorUpdates);
            agent.stats.ModifyHunger(-foodEnergy);
            hasEatenFood = true;

            return BehaviorStatus.Running;
        }
    }
}
