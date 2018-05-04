//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;
using GameAISimulator;
using BehaviorTreeSystem.Enums;

public class AttackTarget : AgentAction
{
    float timeTillAttack;

    public AttackTarget(Agent agentPerformingAction) : base(agentPerformingAction)
    {

    }

    protected override void OnInitialize()
    {
        timeTillAttack = agent.data.AttackSpeed;
    }

    protected override BehaviorStatus OnUpdate()
    {
        //Debug.Log(string.Format("{0}: AttackTarget.", agent.agentName));

        if (agent.target == null)
        {
            Debug.LogWarning("Can't attack a null target.");
            return BehaviorStatus.Failure;
        }

        if (timeTillAttack <= 0)
        {
            IDamageable targetToAttack = agent.target.GetComponent<IDamageable>();
            if (targetToAttack == null)
            {
                Debug.LogError("Target cannot be attacked because it does not have a script that implements IDamageable.");
                return BehaviorStatus.Failure;
            }

            targetToAttack.ReceiveDamage(agent.data.AttackDamage);
            return BehaviorStatus.Success;
        }
        else
        {
            //agent.Chan
            timeTillAttack -= SimulationData.realSecondsBetweenAgentBehaviorUpdates;
            return BehaviorStatus.Running;
        }
    }

    protected override void OnTerminate(BehaviorStatus status)
    {
        
    }
}
