//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;

public class Portal
{
    public readonly Vector3 position;

    public Region RegionOne { get; private set; }
    public Region RegionTwo { get; private set; }

    public EdgeType EdgeType { get; private set; }
    public float Cost { get; private set; }

    public Portal(Vector3 position, Region regionOne, Region regionTwo, EdgeType edgeType)
    {
        this.position = position;
        RegionOne = regionOne;
        RegionTwo = regionTwo;
        EdgeType = edgeType;
        Cost = DetermineCost();
    }

    public Region OtherRegion(Region region)
    {
        if (region == RegionOne)
        {
            return RegionTwo;
        }
        else if (region == RegionTwo)
        {
            return RegionOne;
        }
        else
        {
            Debug.LogWarning("Portal does not contain passed region.");
            return null;
        }
    }

    float DetermineCost()
    {
        float distanceFromRegionOne = Vector3.Distance(position, RegionOne.centerPosition);
        float distanceFromRegionTwo = Vector3.Distance(RegionTwo.centerPosition, position);

        return distanceFromRegionOne + distanceFromRegionTwo;
    }
}
