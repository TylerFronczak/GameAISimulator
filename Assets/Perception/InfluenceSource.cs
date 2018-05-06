//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;
using GameAISimulator.Enums;

public class InfluenceSource : MonoBehaviour
{
    public InfluenceType influenceType;
    public float influence;

    private void OnEnable()
    {
        EventManager.TriggerEvent(CustomEventType.InfluenceSource_Created, this);
    }

    private void OnDisable()
    {
        EventManager.TriggerEvent(CustomEventType.InfluenceSource_Destroyed, this);
    }
}
