//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using BehaviorTreeSystem.Enums;

public class IsDetecting : AgentCondition
{
    readonly string tag;

    public IsDetecting(Agent agent, string tag) : base(agent)
    {
        this.tag = tag;
    }

    protected override BehaviorStatus OnUpdate()
    {
        bool isDetecting = false;

        //Check sight
        if (agent.fieldOfView.IsInView(tag))
        {
            isDetecting = true;
        }

        //Check hearing
        //Check smell

        if (isDetecting) { return BehaviorStatus.Success; }
        else { return BehaviorStatus.Failure; }
    }
}
