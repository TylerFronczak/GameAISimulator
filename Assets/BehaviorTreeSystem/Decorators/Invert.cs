//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using BehaviorTreeSystem.Enums;

namespace BehaviorTreeSystem
{
    public class Invert : Decorator
    {
        protected override BehaviorStatus OnUpdate()
        {
            BehaviorStatus childStatus = child.Tick();
            if (childStatus == BehaviorStatus.Failure)
            {
                child.Reset();
                return BehaviorStatus.Success;
            }
            else if (childStatus == BehaviorStatus.Success)
            {
                child.Reset();
                return BehaviorStatus.Failure;
            }
            else if (childStatus == BehaviorStatus.Running)
            {
                return BehaviorStatus.Running;
            }

            return BehaviorStatus.Invalid;
        }
    }
}
