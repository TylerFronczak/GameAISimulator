//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using BehaviorTreeSystem.Enums;
using GameAISimulator;
using GameAISimulator.Enums;

public class AssignTarget : AgentAction
{
    TargetType targetType;
    TargetCriteria targetCriteria;

    List<string> targetTags = new List<string>();

    public AssignTarget(Agent agentPerformingAction, TargetType targetType, TargetCriteria targetCriteria) : base(agentPerformingAction)
    {
        this.targetType = targetType;
        this.targetCriteria = targetCriteria;
    }

    protected override void OnInitialize()
    {
        targetTags.Clear();

        switch (targetType)
        {
            case TargetType.Agent:
                targetTags.Add(Tags.agent);
                break;
            case TargetType.Food:
                if (agent.data.Diet == Diet.Carnivore)
                {
                    targetTags.Add(Tags.carcass);
                }
                else if (agent.data.Diet == Diet.Herbivore)
                {
                    targetTags.Add(Tags.plant);
                }
                else if (agent.data.Diet == Diet.Omnivore)
                {
                    targetTags.Add(Tags.carcass);
                    targetTags.Add(Tags.plant);
                }
                break;
        }
    }

    protected override BehaviorStatus OnUpdate()
    {
        Transform target = null;
        switch (targetCriteria)
        {
            case TargetCriteria.ClosestInView:
                target = agent.fieldOfView.GetClosestTransformInView(targetTags);
                break;
        }

        if (target != null)
        {
            agent.target = target.gameObject;
            return BehaviorStatus.Success;
        }

        return BehaviorStatus.Failure;
    }
}

public enum TargetCriteria
{
    ClosestInView
}
