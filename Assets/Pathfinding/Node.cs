//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. "Basic A* Pathfing Made Simple" by James Matthews in Game AI Programming Wisdom(Book)
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using GameAISimulator.Enums;

public class Node
{
    public Cell Cell { get; private set; }

    public Vector3 position;

    public List<Edge> Edges { get; private set; }

    bool isVisualizingSearch;
    public bool IsVisualizingSearch
    {
        get {
            return isVisualizingSearch;
        }
        set {
            isVisualizingSearch = value;
            if (isVisualizingSearch)
            {
                Cell.UpdateOutline(searchStatus);
            }

            Cell.ToggleOutline(isVisualizingSearch);
        }
    }

    SearchStatus searchStatus;
    public SearchStatus SearchStatus
    {
        get { return searchStatus; }
        set
        {
            searchStatus = value;
            if (isVisualizingSearch)
            {
                Cell.UpdateOutline(searchStatus);
            }
        }
    }

    /// <summary> Previous node on shortest path to this node. </summary>
    public Node parent;
    public Edge edgeOfParent;

    /// <summary> Lowest cost path from starting node. </summary>
    public float gCost;
    /// <summary> Heuristic, estimated cost to goal. </summary>
    public float hCost;
    /// <summary> Total cost, fCost = gCost + fCost. </summary>
    public float fCost;

    public Node(Cell cell)
    {
        this.position = cell.transform.position;
        Cell = cell;
        Edges = new List<Edge>();
        ResetSearchData();
    }

    public void ResetSearchData()
    {
        SearchStatus = SearchStatus.None;
        parent = null;
        gCost = Mathf.Infinity;
        hCost = Mathf.Infinity;
        fCost = Mathf.Infinity;
    }

    public void RecalculateAllEdgeData()
    {
        for (int i = 0; i < Edges.Count; i++)
        {
            Edges[i].RecalculateEdgeData();
        }
    }
}
