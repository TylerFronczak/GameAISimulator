//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using BehaviorTreeSystem.Enums;

namespace BehaviorTreeSystem
{
    public class BehaviorTree
    {
        public Behavior root;

        public BehaviorTree()
        {
            
        }

        public BehaviorStatus Tick()
        {
            if (root != null)
            {
                return root.Tick();
            }

            return BehaviorStatus.Invalid;
        }
    }
}
