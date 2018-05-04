//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System.Collections.Generic;

namespace BehaviorTreeSystem
{
    /// <summary> Branch with multiple children. </summary>
    public class Composite : Behavior
    {
        protected List<Behavior> children = new List<Behavior>();

        public override void AddChild(Behavior child)
        {
            children.Add(child);
        }

        public void RemoveChild(Behavior child)
        {
            children.Remove(child);
        }

        public void ClearChildren()
        {
            children.Clear();
        }
    }
}
