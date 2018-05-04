//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. https://github.com/aigamedev/btsk/blob/master/BehaviorTree.cpp
//*************************************************************************************************

using BehaviorTreeSystem.Enums;

namespace BehaviorTreeSystem
{
    abstract public class Behavior
    {
        protected virtual void OnInitialize()
        {

        }

        protected virtual BehaviorStatus OnUpdate()
        {
            return BehaviorStatus.Running;
        }

        protected virtual void OnTerminate(BehaviorStatus status)
        {

        }

        private BehaviorStatus status;

        public NodeArchetype nodeArchetype;
        public Behavior parent;
        public int depthInTree;
        public string name;
        public virtual void AddChild(Behavior child)
        {

        }

        public BehaviorStatus Tick()
        {
            if (status != BehaviorStatus.Running) { OnInitialize(); }
            status = OnUpdate();
            if (status != BehaviorStatus.Running) { OnTerminate(status); }
            return status;
        }

        public void Reset()
        {
            status = BehaviorStatus.Invalid;
        }

        public void Terminate()
        {
            OnTerminate(BehaviorStatus.Terminated);
            status = BehaviorStatus.Terminated;
        }

        public bool IsTerminated()
        {
            return status == BehaviorStatus.Success || status == BehaviorStatus.Failure;
        }

        public bool IsRunning()
        {
            return status == BehaviorStatus.Running;
        }

        public BehaviorStatus GetStatus()
        {
            return status;
        }
    }
}
