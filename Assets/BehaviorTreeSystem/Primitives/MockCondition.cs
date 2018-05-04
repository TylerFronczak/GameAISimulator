//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using BehaviorTreeSystem.Enums;

namespace BehaviorTreeSystem
{
    public class MockCondition : Condition
    {
        public BehaviorStatus status;

        public MockCondition(BehaviorStatus status)
        {
            this.status = status;
        }

        protected override BehaviorStatus OnUpdate()
        {
            return status;
        }
    }
}
