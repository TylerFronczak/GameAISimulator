//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;

public class IntentManager : MonoBehaviour
{
    public static IntentManager Instance { get; private set; }

    public List<Intent> Intents { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Intents = new List<Intent>();
            DontDestroyOnLoad(this.gameObject);
        }
        else if (this != Instance)
        {
            Destroy(this.gameObject);
        }
    }

    public void ClearIntents()
    {
        Intents.Clear();
    }

    public void CreateIntent_LoadTerrain(string fileName, bool isUserCreated = false) 
    {
        Intents.Add(new Intent(IntentType.LoadTerrain, fileName, 0, isUserCreated));
    }

    public void CreateIntent_LoadMode(int simulationMode)
    {
        Intents.Add(new Intent(IntentType.LoadMode, string.Empty, simulationMode, false));
    }
}

public struct Intent
{
    public readonly IntentType intentType;
    public readonly string intentString;
    public readonly int intentInt;
    public readonly bool intentBool;

    public Intent(IntentType intentType, string intentString, int intentInt, bool intentBool)
    {
        this.intentType = intentType;
        this.intentString = intentString;
        this.intentInt = intentInt;
        this.intentBool = intentBool;
    }
}

public enum IntentType
{
    LoadTerrain,
    LoadMode
}
