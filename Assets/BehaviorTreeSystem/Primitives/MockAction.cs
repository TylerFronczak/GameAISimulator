//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using BehaviorTreeSystem;
using BehaviorTreeSystem.Enums;

public class MockAction : Action
{
    public BehaviorStatus status;

    public MockAction(BehaviorStatus status)
    {
        this.status = status;
    }

    protected override BehaviorStatus OnUpdate()
    {
        return status;
    }
}
