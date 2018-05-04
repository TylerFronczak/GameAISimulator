//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System;
using BehaviorTreeSystem.Enums;

namespace BehaviorTreeSystem
{
    public class CompareFloats_DynamicToFixed : Condition
    {
        Func<float> dynamicFloat;
        Equality relationshipToCheck;
        float fixedFloat;

        public CompareFloats_DynamicToFixed(Func<float> dynamicFloat, Equality relationshipToCheck, float fixedFloat)
        {
            this.dynamicFloat = dynamicFloat;
            this.relationshipToCheck = relationshipToCheck;
            this.fixedFloat = fixedFloat;
        }

        protected override BehaviorStatus OnUpdate()
        {
            float dynamicFloatValue = dynamicFloat();

            switch (relationshipToCheck)
            {
                case Equality.Equal:
                    if (dynamicFloatValue == fixedFloat) { return BehaviorStatus.Success; }
                    break;
                case Equality.NotEqual:
                    if (dynamicFloatValue != fixedFloat) { return BehaviorStatus.Success; }
                    break;
                case Equality.GreaterThan:
                    if (dynamicFloatValue > fixedFloat) { return BehaviorStatus.Success; }
                    break;
                case Equality.GreaterThanOrEqualTo:
                    if (dynamicFloatValue >= fixedFloat) { return BehaviorStatus.Success; }
                    break;
                case Equality.LessThan:
                    if (dynamicFloatValue < fixedFloat) { return BehaviorStatus.Success; }
                    break;
                case Equality.LessThanOrEqualTo:
                    if (dynamicFloatValue <= fixedFloat) { return BehaviorStatus.Success; }
                    break;
            }

            return BehaviorStatus.Failure;
        }
    }
}
