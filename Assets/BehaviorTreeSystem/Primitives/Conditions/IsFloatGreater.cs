//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System;
using BehaviorTreeSystem.Enums;

namespace BehaviorTreeSystem
{
    /// <summary> Returns success if valueA is greater than valueB. </summary>
    public class IsFloatGreater : Condition
    {
        Func<float> valueA;
        Func<float> valueB;

        public IsFloatGreater(Func<float> valueA, Func<float> valueB)
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
