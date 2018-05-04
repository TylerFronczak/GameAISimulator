//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using BehaviorTreeSystem.Enums;

namespace BehaviorTreeSystem
{
    public class Action : Behavior
    {
        public Action()
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
