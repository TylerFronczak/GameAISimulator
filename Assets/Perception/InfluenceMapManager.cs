//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using GameAISimulator.Enums;

public class InfluenceMapManager : MonoBehaviour
{
    public static InfluenceMapManager Instance { get; private set; }

    public InfluenceMap PopulationMap { get; private set; }

    Pathfinder pathfinder;

    List<Region> regions;
    int regionCount;
    int upperBound;

    Dictionary<int, float> regionDistances;

    [SerializeField] Color minInfluenceColor;
    [SerializeField] Color maxInfluenceColor;

    [SerializeField] GameObject[] populationMapSliderObjects;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize(Pathfinder pathfinder, List<Region> regions)
    {
        this.pathfinder = pathfinder;
        this.regions = regions;
        PopulationMap = new InfluenceMap(InfluenceType.Population, regions, this, 0.5f, 0.5f);

        CalculateRegionDistances();

        propagationTimer = secondsTillPropagation;

        AddListeners();
    }

    void CalculateRegionDistances()
    {
        regionDistances = new Dictionary<int, float>();

        // Custom pairing algorithm for int keys of a bounded set of natural numbers.
        // key = (lowestOfPair * upperBound + 1) + highestOfPair

        regionCount = regions.Count;
        upperBound = regions[regionCount - 1].id;

        for (int startRegionID = 0; startRegionID < regionCount; startRegionID++)
        {
            // The index of goalRegion always starts above startRegion to prevent redundant calculations. 
            for (int goalRegionID = startRegionID + 1; goalRegionID < regionCount; goalRegionID++)
            {
                float distance = 0f;

                RegionPath path = pathfinder.FindRegionPath(regions[startRegionID], regions[goalRegionID], false);

                if (path == null)
                {
                    distance = Mathf.Infinity;
                }
                else
                {
                    for (int i = 0; i < path.points.Count - 1; i++)
                    {
                        distance += Vector3.Distance(path.points[i].position, path.points[i + 1].position);
                    }
                }

                int key = startRegionID * (upperBound + 1) + goalRegionID;
                regionDistances.Add(key, distance);
            }
        }
    }

    public float GetPathDistance(int regionID1, int regionID2)
    {
        int key;

        if (regionID1 < regionID2)
        {
            key = regionID1 * (upperBound + 1) + regionID2;
        }
        else
        {
            key = regionID2 * (upperBound + 1) + regionID1;
        }

        return regionDistances[key];
    }

    float secondsTillPropagation = 0.5f;
    float propagationTimer;

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.U))
        //{
        //    Debug.Log(GetPathDistance(regionOne, regionTwo));
        //}

        //if (Input.GetKeyDown(KeyCode.Y))
        //{
        //    //PopulationMap.AddInfluence(1f, 0);
        //}

        propagationTimer += Time.deltaTime;
        if (propagationTimer >= secondsTillPropagation)
        {
            PopulationMap.PropagateInfluence();
            propagationTimer = 0;
        }
    }

    [SerializeField] bool isVisualizingInfluence;

    readonly string[] decimalValues = new string[]
    {
        "0", "0.01", "0.02", "0.03", "0.04", "0.05", "0.06", "0.07", "0.08", "0.09",
        "0.1", "0.11", "0.12", "0.13", "0.14", "0.15", "0.16", "0.17", "0.18", "0.19",
        "0.2", "0.21", "0.22", "0.23", "0.24", "0.25", "0.26", "0.27", "0.28", "0.29",
        "0.3", "0.31", "0.32", "0.33", "0.34", "0.35", "0.36", "0.37", "0.38", "0.39",
        "0.4", "0.41", "0.42", "0.43", "0.44", "0.45", "0.46", "0.47", "0.48", "0.49",
        "0.5", "0.51", "0.52", "0.53", "0.54", "0.55", "0.56", "0.57", "0.58", "0.59",
        "0.6", "0.61", "0.62", "0.63", "0.64", "0.65", "0.66", "0.67", "0.68", "0.69",
        "0.7", "0.71", "0.72", "0.73", "0.74", "0.75", "0.76", "0.77", "0.78", "0.79",
        "0.8", "0.81", "0.82", "0.83", "0.84", "0.85", "0.86", "0.87", "0.88", "0.89",
        "0.9", "0.91", "0.92", "0.93", "0.94", "0.95", "0.96", "0.97", "0.98", "0.99",
        "1"
    };

    public void UpdateInfluenceVisualization(InfluenceType mapType)
    {
        if (isVisualizingInfluence)
        {
            if (mapType == InfluenceType.Population)
            {
                for (int i = 0; i < regionCount; i++)
                {
                    float influenceValue = PopulationMap.InfluencesValues[i];
                    Color newColor = Color.Lerp(minInfluenceColor, maxInfluenceColor, influenceValue);
                    regions[i].ModifyUI_Color(newColor);

                    int decimalValueIndex = Mathf.RoundToInt(influenceValue * 100);
                    regions[i].ModifyUI_Text(decimalValues[decimalValueIndex]);
                }
            }
        }
    }

    #region TempVisualziationPurposes

    public void ModifyPopMap_Momentum(float value)
    {
        value = Mathf.Clamp(value, 0f, 1f);
        PopulationMap.momentum = value;
    }

    public void ModifyPopMap_Decay(float value)
    {
        value = Mathf.Clamp(value, 0f, 1f);
        PopulationMap.decay = value;
    }

    public void Toggle_IsVisualizingInfluence()
    {
        isVisualizingInfluence = !isVisualizingInfluence;
    }

    public void TogglePopulationMapOptions(bool isEnabled)
    {
        for (int i = 0; i < populationMapSliderObjects.Length; i++)
        {
            populationMapSliderObjects[i].SetActive(isEnabled);
        }
    }
    #endregion

    #region EventSystem
    private void AddListeners()
    {
        EventManager.StartListening(CustomEventType.InfluenceSource_Created, InfluenceSourceCreated);
        EventManager.StartListening(CustomEventType.InfluenceSource_Destroyed, InfluenceSourceDestroyed);
    }
    private void RemoveListeners()
    {
        EventManager.StopListening(CustomEventType.InfluenceSource_Created, InfluenceSourceCreated);
        EventManager.StopListening(CustomEventType.InfluenceSource_Destroyed, InfluenceSourceDestroyed);
    }

    private void InfluenceSourceCreated<Object>(Object influenceSourceObject)
    {
        InfluenceSource influenceSource = influenceSourceObject as InfluenceSource;
        if (influenceSource.influenceType == InfluenceType.Population)
        {
            PopulationMap.AddInfluenceSource(influenceSource);
        }
    }

    private void InfluenceSourceDestroyed<Object>(Object influenceSourceObject)
    {
        InfluenceSource influenceSource = influenceSourceObject as InfluenceSource;
        if (influenceSource.influenceType == InfluenceType.Population)
        {
            PopulationMap.RemoveInfluenceSource(influenceSource);
        }
    }

    private void OnDisable()
    {
        RemoveListeners();
    }
    #endregion
}
