//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;
using BehaviorTreeSystem.Enums;

public class HasTarget : AgentCondition
{
    readonly TargetType targetType;

    public HasTarget(Agent agent, TargetType targetType) : base(agent)
    {
        this.targetType = targetType;
    }

    protected override BehaviorStatus OnUpdate()
    {
        ITarget currentTarget;
        if (agent.target != null)
        {
            currentTarget = agent.target.GetComponentInParent<ITarget>();
            if (currentTarget != null && currentTarget.TargetType == targetType)
            {
                Debug.Log("Already haave target Food!!!");
                return BehaviorStatus.Success;
            }
        }

        return BehaviorStatus.Failure;
    }
}
