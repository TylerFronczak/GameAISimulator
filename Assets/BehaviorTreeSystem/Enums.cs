//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

namespace BehaviorTreeSystem
{
    namespace Enums
    {
        public enum BehaviorStatus
        {
            Failure,
            Invalid,
            Running,
            Success,
            Terminated
        }
        public static class BehaviorStatusExtensions
        {
            public static string ElementToString(this BehaviorStatus behaviorStatus)
            {
                switch (behaviorStatus)
                {
                    case BehaviorStatus.Failure:
                        return "Failure";
                    case BehaviorStatus.Invalid:
                        return "Invalid";
                    case BehaviorStatus.Running:
                        return "Running";
                    case BehaviorStatus.Success:
                        return "Success";
                    case BehaviorStatus.Terminated:
                        return "Terminated";
                    default:
                        return "Unrecognized BehaviorStatus";
                }
            }
        }

        public enum NodeArchetype
        {
            Leaf,
            Decorator,
            Composite
        }

        public enum DecoratorType
        {
            /// <summary> Prints ID and status of child node. </summary>
            Debug,
            /// <summary> Returns a success status once child node is finish running. </summary>
            ForceStatus,
            /// <summary> Swaps the child node success status from success to fail and vice versa. </summary>
            Invert,
            /// <summary> Processes the child node a specified number of times. </summary>
            Loop,
            /// <summary> Repeats processing of child node until a success status is returned. </summary>
            Retry,
            /// <summary> If processing of child node exceeds specified time, the child processing is terminated and a fail status is returned. </summary>
            TimeLimit,
            /// <summary> Delays processing of child node until the specified time has expired. Timer starts upon first visit and resets with each subsequent visit. </summary>
            Wait_Seconds,
            /// <summary> Delays processing of child node until the specified number of ticks have occured. Timer starts upon first visit and resets with each subsequent visit. </summary>
            Wait_Ticks,
            Unrecognized
        }
        public static class DecoratorTypeExtensions
        {
            public static string ElementToString(this DecoratorType decoratorType)
            {
                switch (decoratorType)
                {
                    case DecoratorType.Debug:
                        return "Debug";
                    case DecoratorType.ForceStatus:
                        return "ForceStatus";
                    case DecoratorType.Invert:
                        return "Invert";
                    case DecoratorType.Loop:
                        return "Loop";
                    case DecoratorType.Retry:
                        return "Retry";
                    case DecoratorType.TimeLimit:
                        return "TimeLimit";
                    case DecoratorType.Wait_Seconds:
                        return "Wait_Seconds";
                    case DecoratorType.Wait_Ticks:
                        return "Wait_Ticks";
                    default:
                        return "Unrecognized DeceratorType";
                }
            }
        }
        public static class StringToDecoratorType
        {
            public static DecoratorType GetType(string type)
            {
                switch (type)
                {
                    case "Debug":
                        return DecoratorType.Debug;
                    case "ForceStatus":
                        return DecoratorType.ForceStatus;
                    case "Invert":
                        return DecoratorType.Invert;
                    case "Loop":
                        return DecoratorType.Loop;
                    case "Retry":
                        return DecoratorType.Retry;
                    case "TimeLimit":
                        return DecoratorType.TimeLimit;
                    case "Wait_Seconds":
                        return DecoratorType.Wait_Seconds;
                    case "Wait_Ticks":
                        return DecoratorType.Wait_Ticks;
                    default:
                        return DecoratorType.Unrecognized;
                }
            }
        }

        public enum SelectorType
        {
            LeftToRight,
            Priority,
            Random
        }

        public enum Equality
        {
            Equal,
            NotEqual,
            GreaterThan,
            LessThan,
            GreaterThanOrEqualTo,
            LessThanOrEqualTo
        }
    }
}
