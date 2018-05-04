//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using BehaviorTreeSystem.Enums;

namespace BehaviorTreeSystem
{
    /// <summary> aka Loop. Success, if looping of child reaches specified limit.
    /// Failure, if child returns failure.
    /// Invalid, if child returns running.
    /// </summary>
    public class Repeat : Decorator
    {
        private int maxRepitions;
        public int repetitionCount;

        /// <summary> A passed value of -1 will cause inifinite looping until failure or invalid is returned. </summary>
        public Repeat(int maxRepitions)
        {
            this.maxRepitions = maxRepitions;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            repetitionCount = -1;
        }

        protected override BehaviorStatus OnUpdate()
        {
            while (true)
            {
                child.Tick();
                repetitionCount++;

                if (child.IsRunning()) { break; }
                else if (child.GetStatus() == BehaviorStatus.Failure) { return BehaviorStatus.Failure; }
                else if (repetitionCount == maxRepitions) { return BehaviorStatus.Success; }

                child.Reset();
            }
            return BehaviorStatus.Invalid;
        }
    }
}
