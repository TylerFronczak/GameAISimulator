//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using BehaviorTreeSystem;

public class AgentCondition : Condition
{
    protected readonly Agent agent;

    public AgentCondition(Agent agent)
    {
        this.agent = agent;
    }
}
