//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using BehaviorTreeSystem.Enums;

namespace BehaviorTreeSystem
{
    /// <summary> Returns success if amy child returns success. </summary>
    public class Selector : Composite
    {
        public Selector()
        {
            nodeArchetype = NodeArchetype.Composite;
        }

        protected Behavior currentChild;
        private int childIndex;

        protected override void OnInitialize()
        {
            currentChild = children[0];
            childIndex = 0;
        }

        protected override BehaviorStatus OnUpdate()
        {
            while (true)
            {
                BehaviorStatus status = currentChild.Tick();

                if (status != BehaviorStatus.Failure)
                {
                    return status;
                }
                else if (++childIndex == children.Count)
                {
                    return BehaviorStatus.Failure;
                }
                else
                {
                    currentChild = children[childIndex];
                }
            }
        }
    }
}
