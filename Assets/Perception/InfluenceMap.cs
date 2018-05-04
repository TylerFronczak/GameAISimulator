//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. http://aigamedev.com/open/tutorial/influence-map-mechanics/#Conclusion
//  2. https://sciencing.com/write-linear-decay-function-8646603.html
//  3. "Influence Mapping" by Paul Tozour in Game Programming Gems 2
//
// Notes: 
//  1. Consider the implications of additive and reference influence sources.
//     Reference influence sources might allow less updating and propagation.
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using GameAISimulator.Enums;

public class InfluenceMap
{
    public InfluenceType MapType { get; private set; }

    public List<Region> Regions { get; private set; }

    public float[] InfluencesValues { get; private set; }

    public float tempInfluencePerSource = 0.25f;
    public float momentum; // value between ------------
    public float decay; // aka falloff constant, newInfluence = sourceInfluence / (decay * distance + 1)
    float minInfluence = 0f;
    float maxInfluence = 1f;
    List<InfluenceSource> influenceSources;

    InfluenceMapManager influenceMapManager;

    public InfluenceMap(InfluenceType type, List<Region> regions, InfluenceMapManager influenceMapManager, float momentum, float decay)
    {
        MapType = type;

        Regions = regions;
        InfluencesValues = new float[Regions.Count];
        tempInfluenceValues = new float[Regions.Count];

        influenceSources = new List<InfluenceSource>();

        this.influenceMapManager = influenceMapManager;

        this.momentum = momentum;
        this.decay = decay;
    }

    private float[] tempInfluenceValues;

    public void PropagateInfluence()
    {
        if (influenceSources.Count == 0)
        {
            return;
        }

        RegionManager regionManager = RegionManager.Instance;

        for (int i = 0; i < tempInfluenceValues.Length; i++)
        {
            tempInfluenceValues[i] = 0f;
        }

        for (int i = 0; i < influenceSources.Count; i++)
        {
            int sourceRegionID = regionManager.GetRegionID(influenceSources[i].transform.position);

            tempInfluenceValues[sourceRegionID] += tempInfluencePerSource;

            for (int regionID = 0; regionID < Regions.Count; regionID++)
            {
                if (regionID == sourceRegionID)
                {
                    continue;
                }

                float pathDistanceBetweenRegions = influenceMapManager.GetPathDistance(sourceRegionID, regionID);

                if (pathDistanceBetweenRegions != Mathf.Infinity)
                {
                    tempInfluenceValues[regionID] += tempInfluencePerSource / (decay * pathDistanceBetweenRegions + 1);
                }
                else
                {
                    tempInfluenceValues[regionID] = InfluencesValues[regionID];
                }
            }
        }

        for (int regionID = 0; regionID < Regions.Count; regionID++)
        {
            tempInfluenceValues[regionID] = Mathf.Clamp(tempInfluenceValues[regionID], minInfluence, maxInfluence);
            InfluencesValues[regionID] = Mathf.Lerp(InfluencesValues[regionID], tempInfluenceValues[regionID], momentum);
        }

        influenceMapManager.UpdateInfluenceVisualization(MapType);
    }

    public void AddInfluenceSource(InfluenceSource source)
    {
        influenceSources.Add(source);
    }

    public void RemoveInfluenceSource(InfluenceSource source)
    {
        influenceSources.Remove(source);
    }
}
