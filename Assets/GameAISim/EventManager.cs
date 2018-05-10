//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. https://unity3d.com/learn/tutorials/topics/scripting/events-creating-simple-messaging-system
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    private Dictionary<CustomEventType, CustomUnityEvent> eventDictionary;

    private static EventManager eventManager;
    public static EventManager Instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                if (!eventManager)
                {
                    GameObject instanceObject = new GameObject("EventManager");
                    eventManager = instanceObject.AddComponent<EventManager>();
                }

                eventManager.Initialize();
            }

            return eventManager;
        }
    }

    private void Initialize()
    {
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<CustomEventType, CustomUnityEvent>();
        }
    }

    public static void StartListening(CustomEventType eventType, UnityAction<Object> listener)
    {
        CustomUnityEvent unityEvent = null;
        if (!Instance.eventDictionary.TryGetValue(eventType, out unityEvent))
        {
            unityEvent = new CustomUnityEvent();
            Instance.eventDictionary.Add(eventType, unityEvent);
        }

        unityEvent.AddListener(listener);
    }

    public static void StopListening(CustomEventType eventType, UnityAction<Object> listener)
    {
        if (eventManager == null) { return; }

        CustomUnityEvent unityEvent = null;
        if (Instance.eventDictionary.TryGetValue(eventType, out unityEvent))
        {
            unityEvent.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(CustomEventType eventType, Object actionObject)
    {
        if (!isApplicationQuitting)
        {
            CustomUnityEvent unityEvent = null;
            if (Instance.eventDictionary.TryGetValue(eventType, out unityEvent))
            {
                unityEvent.Invoke(actionObject);
            }
        }
    }

    private static bool isApplicationQuitting;
    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }
}

public enum CustomEventType
{
    AgentDeath,
    AgentSelection,
    ClickedCell,
    Food_Created,
    Food_Depleted,
    InfluenceSource_Created,
    InfluenceSource_Destroyed,
    SceneExit
}

[System.Serializable]
public class CustomUnityEvent : UnityEvent<Object>
{

}
