//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

public interface ITarget
{
    TargetType TargetType { get; }
}

public enum TargetType
{
    Agent,
    Food
}
