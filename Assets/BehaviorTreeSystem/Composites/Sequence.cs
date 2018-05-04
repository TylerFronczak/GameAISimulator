//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using BehaviorTreeSystem.Enums;

namespace BehaviorTreeSystem
{
    /// <summary> Returns success if all children return success. </summary>
    public class Sequence : Composite
    {
        public Sequence()
        {
            nodeArchetype = NodeArchetype.Composite;
        }

        protected Behavior currentChild;
        private int childIndex;

        protected override void OnInitialize()
        {
            childIndex = 0;
            currentChild = children[childIndex];
        }

        protected override BehaviorStatus OnUpdate()
        {
            while (true)
            {
                BehaviorStatus status = currentChild.Tick();
                if (status != BehaviorStatus.Success)
                {
                    return status;
                }
                else if (++childIndex == children.Count)
                {
                    return BehaviorStatus.Success;
                }
                else
                {
                    currentChild = children[childIndex];
                }
            }
        }
    }
}
