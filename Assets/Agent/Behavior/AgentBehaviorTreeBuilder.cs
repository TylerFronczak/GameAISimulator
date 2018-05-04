//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using BehaviorTreeSystem;
using BehaviorTreeSystem.Enums;
using GameAISimulator;
using GameAISimulator.Enums;

public class AgentBehaviorTreeBuilder : BehaviorTreeBuilder<AgentBehaviorTree>
{
    private Agent agent;

    public AgentBehaviorTreeBuilder(Behavior root, Agent agent) : base(root)
    {
        this.agent = agent;
    }

    public AgentBehaviorTreeBuilder Condition(ConditionType type)
    {
        Condition condition = null;

        switch (type)
        {
            case ConditionType.IsDetecting_Agent:
                condition = new IsDetecting(agent, Tags.agent);
                break;
            case ConditionType.IsDetecting_Carcass:
                condition = new IsDetecting(agent, Tags.carcass);
                break;
            case ConditionType.IsDetecting_Plant:
                condition = new IsDetecting(agent, Tags.plant);
                break;
            case ConditionType.IsDetecting_Violence:
                condition = new IsDetecting(agent, Tags.violence);
                break;

            case ConditionType.IsHealth_AboveHalf:
                condition = new CompareFloats_DynamicToFixed(agent.stats.GetCurrentHealthToMaxRatio, Equality.GreaterThan, 0.5f);
                break;

            case ConditionType.IsStatus_Starving:
                condition = new CompareFloats_DynamicToFixed(agent.stats.GetCurrentHungerToMaxRatio, Equality.GreaterThanOrEqualTo, 1f);
                break;

            case ConditionType.HasTarget_Food:
                condition = new HasTarget(agent, TargetType.Food);
                break;
        }

        base.Condition(condition);
        return this;
    }

    public AgentBehaviorTreeBuilder Action(ActionType type)
    {
        Action action = null;

        switch (type)
        {
            case ActionType.AssignTarget_Agent_ClosestInView:
                action = new AssignTarget(agent, TargetType.Agent, TargetCriteria.ClosestInView);
                break;
            case ActionType.AssignTarget_Food_ClosestInView:
                action = new AssignTarget(agent, TargetType.Food, TargetCriteria.ClosestInView);
                break;

            case ActionType.Eat:
                action = new Eat(agent);
                break;

            case ActionType.AttackTarget:
                action = new AttackTarget(agent);
                break;
            case ActionType.MoveToTarget:
                action = new MoveToTarget(agent);
                break;
            case ActionType.SteerToTarget:
                action = new SteerToTarget(agent);
                break;

            case ActionType.Wander:
                action = new Wander(agent);
                break;

            case ActionType.ChangeColor_Aggressive:
                action = new ChangeColorStatus(agent, ColorStatus.Aggressive);
                break;
            case ActionType.ChangeColor_Passive:
                action = new ChangeColorStatus(agent, ColorStatus.Passive);
                break;
        }

        base.Action(action);
        return this;
    }

    new public AgentBehaviorTreeBuilder Decorator(Decorator decorator)
    {
        base.Decorator(decorator);
        return this;
    }

    new public AgentBehaviorTreeBuilder Selector(string name)
    {
        base.Selector(name);
        return this;
    }

    new public AgentBehaviorTreeBuilder Sequence(string name)
    {
        base.Sequence(name);
        return this;
    }

    new public AgentBehaviorTreeBuilder End()
    {
        base.End();
        return this;
    }
}
