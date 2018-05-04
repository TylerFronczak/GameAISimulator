//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameAISimulator.Enums;

public class Region
{
    public readonly int id;

    public readonly RegionQualifier qualifier;

    public List<int> cellIndices;
    public List<int> topIndices;
    public List<int> rightIndices;

    public int CellIndex_BottomLeft { get; private set; }

    public Vector3 centerPosition;

    bool isUIEnabled;
    public RectTransform RegionUI { get; private set; }
    Image uiImage;
    public LineRenderer LineRenderer { get; private set; }

    public List<Portal> portals;

    public Region(int id, RegionQualifier qualifier, int firstCellIndex)
    {
        this.id = id;
        this.qualifier = qualifier;
        cellIndices = new List<int>();
        cellIndices.Add(firstCellIndex);
        CellIndex_BottomLeft = firstCellIndex;
    }

    public void FinishSettingRegion(Vector3 centerPosition, List<int> topIndices, List<int> rightIndices, RectTransform uiRect, Canvas canvas)
    {
        this.centerPosition = centerPosition;
        this.topIndices = topIndices;
        this.rightIndices = rightIndices;
        InitializeRegionUI(uiRect, canvas);

        portals = new List<Portal>();
    }

    void InitializeRegionUI(RectTransform uiRect, Canvas canvas)
    {
        RegionUI = uiRect;
        RegionUI.SetParent(canvas.transform, false);
        RegionUI.position = centerPosition;
        float width = topIndices.Count * CellMetrics.offset;
        float length = rightIndices.Count * CellMetrics.offset;
        RegionUI.sizeDelta = new Vector2(width, length);
        uiText = RegionUI.GetComponentInChildren<Text>();
        uiText.text = id.ToString();
        RegionUI.name = "RegionUI " + "(ID: " + id + ")";

        uiImage = RegionUI.GetComponentInChildren<Image>();
        defualtColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.5f);
        uiImage.color = defualtColor;

        LineRenderer = RegionUI.GetComponentInChildren<LineRenderer>();

        ToggleUI(false);
    }

    public void ToggleUI(bool isEnabled)
    {
        RegionUI.gameObject.SetActive(isEnabled);
    }

    Color defualtColor;
    Text uiText;

    public void ModifyUI_Color(Color color)
    {
        uiImage.color = color;
    }

    public void ModifyUI_Text(string text)
    {
        uiText.text = text;
    }

    #region Pathing
    /// <summary> Lowest cost path from starting region. </summary>
    public float gCost;
    /// <summary> Heuristic, estimated cost to goal. </summary>
    public float hCost;
    /// <summary> Total cost, fCost = gCost + fCost. </summary>
    public float fCost;

    /// <summary> Previous region on shortest path to this region. </summary>
    public Region parent;
    public Portal portalOfParent;

    SearchStatus searchStatus;
    public SearchStatus SearchStatus
    {
        get { return searchStatus; }
        set
        {
            searchStatus = value;
            //if (isVisualizingSearch)
            //{
            //    Cell.UpdateOutline(searchStatus);
            //}
        }
    }

    public void ResetSearchData()
    {
        SearchStatus = SearchStatus.None;
        parent = null;
        gCost = Mathf.Infinity;
        hCost = Mathf.Infinity;
        fCost = Mathf.Infinity;
    }

    #endregion
}
