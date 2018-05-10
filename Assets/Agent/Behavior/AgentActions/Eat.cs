//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;
using BehaviorTreeSystem.Enums;
using GameAISimulator;

public class Eat : AgentAction
{
    Food targetFood;

    bool hasEatenFood;

    Cell targetCell;

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
            if (targetFood == null)
            {
                Debug.LogWarning("Target food is null.");
                return;
            }
            else if (targetFood.isDepleted)
            {
                targetFood = null;
                Debug.LogWarning("Food was depleted before the agent was able to start eating.");
                return;
            }

            targetCell = targetFood.Cell;
            hasEatenFood = false;
        }
    }

    protected override BehaviorStatus OnUpdate()
    {
        //Debug.Log(string.Format("{0}: Eat.", agent.agentName));

        if (targetFood == null || targetFood.isDepleted)
        {
            agent.target = null;

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
            // Must check whether food has moved because the agent's food source could become depleted
            // and then reused by the pooling system before the agent is able to recognize the situation.
            if (targetFood.Cell != targetCell)
            {
                agent.target = null;
                return BehaviorStatus.Failure;
            }

            float foodEnergy = targetFood.DrainEnergy(agent.stats.metabolicRate, 1f, SimulationData.simDaysBetweenBehaviorUpdates);
            agent.stats.ModifyHunger(-foodEnergy);
            hasEatenFood = true;

            return BehaviorStatus.Running;
        }
    }
}
