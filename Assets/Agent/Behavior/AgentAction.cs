//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using BehaviorTreeSystem;

public class AgentAction : Action
{
    protected readonly Agent agent;

    public AgentAction(Agent agentPerformingAction)
    {
        this.agent = agentPerformingAction;
    }
}
