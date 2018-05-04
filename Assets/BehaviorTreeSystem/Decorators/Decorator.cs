//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using BehaviorTreeSystem.Enums;

namespace BehaviorTreeSystem
{
    /// <summary> Changes the processing of its single child node. </summary>
    public class Decorator : Behavior
    {
        protected Behavior child;

        public Decorator()
        {
            nodeArchetype = NodeArchetype.Decorator;
        }

        public override void AddChild(Behavior child)
        {
            this.child = child;
        }
    }
}
