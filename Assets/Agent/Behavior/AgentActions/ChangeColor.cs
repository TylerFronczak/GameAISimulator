//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using BehaviorTreeSystem.Enums;

public class ChangeColorStatus : AgentAction
{
    ColorStatus colorStatus;

    public ChangeColorStatus(Agent agentPerformingAction, ColorStatus colorStatus) : base(agentPerformingAction)
    {
        switch (colorStatus)
        {
            case ColorStatus.Aggressive:
                this.colorStatus = ColorStatus.Aggressive;
                break;
            case ColorStatus.Passive:
                this.colorStatus = ColorStatus.Passive;
                break;
        }
    }

    protected override void OnInitialize()
    {

    }

    protected override BehaviorStatus OnUpdate()
    {
        switch (colorStatus)
        {
            case ColorStatus.Aggressive:
                agent.simpleVehicleModel.ChangeMaterialAggressive();
                break;
            case ColorStatus.Passive:
                agent.simpleVehicleModel.ChangeMaterialDefualt();
                break;
        }

        return BehaviorStatus.Success;
    }
}

public enum ColorStatus
{
    Aggressive,
    Passive
}
