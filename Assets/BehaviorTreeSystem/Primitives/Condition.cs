//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using BehaviorTreeSystem.Enums;

namespace BehaviorTreeSystem
{
    public class Condition : Behavior
    {
        public Condition()
        {
            nodeArchetype = NodeArchetype.Leaf;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        protected override void OnTerminate(BehaviorStatus status)
        {
            base.OnTerminate(status);
        }
    }
}
