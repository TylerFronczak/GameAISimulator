//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using BehaviorTreeSystem.Enums;

namespace BehaviorTreeSystem
{
    public class BehaviorTreeBuilder<T> where T : BehaviorTree, new()
    {
        private T behaviorTree;
        private Behavior buildNode;

        public BehaviorTreeBuilder(Behavior root)
        {
            behaviorTree = new T();
            root.parent = null;
            root.depthInTree = 0;
            behaviorTree.root = root;
            buildNode = root;
        }

        public BehaviorTreeBuilder<T> Condition(Condition condition)
        {
            buildNode.AddChild(condition);
            condition.depthInTree = buildNode.depthInTree + 1;
            condition.parent = buildNode;

            if (buildNode.nodeArchetype == NodeArchetype.Decorator)
            {
                End();
            }

            return this;
        }

        public BehaviorTreeBuilder<T> Action(Action action)
        {
            buildNode.AddChild(action);
            action.depthInTree = buildNode.depthInTree + 1;
            action.parent = buildNode;

            if (buildNode.nodeArchetype == NodeArchetype.Decorator)
            {
                End();
            }

            return this;
        }

        public BehaviorTreeBuilder<T> Decorator(Decorator decerator)
        {
            buildNode.AddChild(decerator);
            decerator.parent = buildNode;
            decerator.depthInTree = buildNode.depthInTree + 1;
            buildNode = decerator;
            return this;
        }

        public BehaviorTreeBuilder<T> Sequence(string name)
        {
            Sequence sequence = new Sequence();
            buildNode.AddChild(sequence);
            sequence.parent = buildNode;
            sequence.depthInTree = buildNode.depthInTree + 1;
            buildNode = sequence;
            return this;
        }

        public BehaviorTreeBuilder<T> Selector(string name)
        {
            Selector selector = new Selector();
            buildNode.AddChild(selector);
            selector.parent = buildNode;
            selector.depthInTree = buildNode.depthInTree + 1;
            buildNode = selector;
            return this;
        }

        /// <summary> Signifies last child of parent, which causes the build node to be reassigned. </summary>
        public BehaviorTreeBuilder<T> End()
        {
            do
            {
                buildNode = buildNode.parent;
            }
            while (buildNode.nodeArchetype == NodeArchetype.Decorator);

            return this;
        }

        public T Build()
        {
            return behaviorTree;
        }
    }
}
