//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System;
using BehaviorTreeSystem.Enums;

namespace BehaviorTreeSystem
{
    /// <summary> Returns success if valueA is greater than valueB. </summary>
    public class IsIntGreater : Condition
    {
        Func<int> valueA;
        Func<int> valueB;

        public IsIntGreater(Func<int> valueA, Func<int> valueB)
        {
            this.valueA = valueA;
            this.valueB = valueB;
        }

        protected override BehaviorStatus OnUpdate()
        {
            if (valueA() > valueB())
            {
                return BehaviorStatus.Success;
            }
            else
            {
                return BehaviorStatus.Failure;
            }
        }
    }
}
