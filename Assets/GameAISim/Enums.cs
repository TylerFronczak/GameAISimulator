//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

namespace GameAISimulator.Enums
{
    public enum Direction
    {
        None, N, NE, E, SE, S, SW, W, NW
    }

    public static class DirectionExtensions
    {
        public static Direction Opposite(this Direction direction)
        {
            if ((int)direction < 4) {
                return direction + 4;
            } else {
                return direction - 4;
            }
        }
    }

    public enum RelativeDirection
    {
        T, UR, R, LR, B, LL, L, UL
    }


    public enum TopType
    {
        Flat,
        Slope,
        Corner
    }

    public enum AtlasTexture
    {
        Grass0=00, Grass1=10, Grass2=20, Grass3=30, Grass4=40, Grass5=50,
        Dirt0=01, Dirt1=11, Dirt2=21,
        Mud0=02, Mud1=12, Mud2=22,
        Sand0=03, Sand1=13, Sand2=23, Sand3=33, Sand4=43,
        Snow0=04, Snow1=14, Snow2=24, Snow3=34
    }


    public enum SearchStatus
    {
        None,
        OpenList,
        ClosedList,
        OnPath
    }

    public enum PathPointType
    {
        Node,
        Edge,
        Curve
    }

    public enum RegionPathPointType
    {
        Region,
        Portal
    }

    public enum RegionQualifier
    {
        None,
        Elevation
    }

    public enum InfluenceType
    {
        Population,
        TerritoryControl
    }

    public enum Diet
    {
        Carnivore,
        Herbivore,
        Omnivore
    }

    public enum ObservableStatStatus
    {
        Max, // 100%
        High, // >66%
        Medium, // >33%
        Low, // <= 33%
        None // 0%
    }

    public enum TurningStatus
    {
        None,
        CW,
        CCW
    }

    public enum AgentBehavior
    {
        Eating,
        Foraging
    }

    public enum PassiveEnergyDeltaType
    {
        None,
        Regenerative,
        Degenerative
    }


    public enum ActionType
    {
        MoveToTarget,
        SteerToTarget,
        AttackTarget,

        Eat,
        Wander,
        Flee,
        FollowTargetAgent,

        AssignTarget_Agent_ClosestInView,
        AssignTarget_Food_ClosestInView,
        AssignTargetAgent_Wounded,

        LookAt,
        StopMoving,
        ResumeMoving,

        ChangeColor_Aggressive,
        ChangeColor_Passive
    }

    public enum ConditionType
    {
        IsDetecting_Agent,
        IsDetecting_Carcass,
        IsDetecting_Plant,
        IsDetecting_Food,
        IsDetecting_Violence,

        IsHealth_AboveHalf,
        IsHealth_Full,

        IsStatus_Dehydrated,
        IsStatus_Starving,

        IsRegion_HighlyPopulated,

        IsTargetAgent_Aware,
        IsTargetAgent_Faster,
        IsTargetAgent_Fleeing,
        IsTargetAgent_Wounded,

        HasTarget_Food
    }


    public enum MovementMode
    {
        None,
        Pathing,
        Steering
    }


    public enum SimulationMode
    {
        Pathfinding,
        Steering,
        Ecosystem
    }
}
