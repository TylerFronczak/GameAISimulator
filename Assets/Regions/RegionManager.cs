//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. "Area Navigation: Expanding the Path-Finding Paradigm" by Ben Board & Mike Ducker in Game Programming Gems 3
//  2. https://stackoverflow.com/questions/222598/how-do-i-clone-a-generic-list-in-c
//  3. https://stackoverflow.com/questions/10901020/which-is-faster-clear-collection-or-instantiate-new
//
// Notes:
//  1. The expansion checks are convoluted, but neccessary unless a fundamental change is made.
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using GameAISimulator.Enums;

public class RegionManager : MonoBehaviour
{
    public static RegionManager Instance { get; private set; }

    public List<Region> Regions { get; private set; }

    private CellGrid cellGrid;

    [SerializeField] RectTransform regionUIPrefab;

    private GameObject collidersGameObject;
    private int numCollidersCreated;

    private void Awake()
    {
        Instance = this;
    }

    public bool IsInitialized { get; private set; }

    public void Initialize(CellGrid cellGrid, Canvas canvas)
    {
        if (!IsInitialized)
        {
            IsInitialized = true;
            Regions = new List<Region>();
            this.cellGrid = cellGrid;
            SegmentGridByElevation(canvas);
            CalculateAllRegionPortals();
            GenerateColliders();
        }
        else
        {
            Debug.LogWarning("RegionManager.Initialize() has already been called!");
        }
    }

    void SegmentGridByElevation(Canvas canvas)//, int maxAreaSize)
    {
        // Ensures the maximum size of a region does not exceed the area to be partitioned.v
        //if (maxAreaSize > Mathf.Sqrt(cellGrid.Cells.Length))
        //{
        //    maxAreaSize = (int)Mathf.Sqrt(cellGrid.Cells.Length);
        //}

        //for (int testSize = 1; testSize < maxAreaSize; testSize++)
        //{

        //}

        int cellIndex = 0; // regionBottomLeftCellIndex
        int regionID = 0;

        // For every cell from bottom-left to top-right
        for (int row = 0; row < cellGrid.Rows; row++)
        {
            for (int column = 0; column < cellGrid.Columns; column++)
            {
                // If this cell's regionID is not -1, it already has a region and should be skipped.
                if (cellGrid.Cells[cellIndex].regionID != -1)
                {
                    cellIndex++;
                    continue;
                }

                regionID = Regions.Count;
                cellGrid.Cells[cellIndex].regionID = regionID;

                int regionElevation = cellGrid.Cells[cellIndex].Elevation;
                Region region = new Region(regionID, RegionQualifier.Elevation, cellIndex);
                Regions.Add(region);

                List<int> topIndices = new List<int>();
                topIndices.Add(cellIndex);

                List<int> rightIndices = new List<int>();
                rightIndices.Add(cellIndex);

                List<int> tempIndices = new List<int>();

                int topRightIndex = 0;

                bool canExpandNorth = CanExpandNorth(topIndices, regionElevation);

                bool canExpandEast = false;
                if (!canExpandNorth) // If North is expanded, CanExpandEast() will be called automatically.
                {
                    canExpandEast = CanExpandEast(rightIndices, regionElevation);
                }

                while (canExpandNorth || canExpandEast)
                {
                    if (canExpandNorth)
                    {
                        for (int i = 0; i < topIndices.Count; i++)
                        {
                            int cellIndexToAdd = topIndices[i] + cellGrid.Columns;
                            region.cellIndices.Add(cellIndexToAdd);
                            cellGrid.Cells[cellIndexToAdd].regionID = regionID;
                            tempIndices.Add(cellIndexToAdd);
                        }

                        topIndices = new List<int>(tempIndices);
                        tempIndices.Clear();

                        // The last index in the top indices would be the region's top-right corner, which needs to be added to the right indices.
                        topRightIndex = topIndices[topIndices.Count - 1];
                        rightIndices.Add(topRightIndex);

                        canExpandEast = CanExpandEast(rightIndices, regionElevation);
                    }

                    if (canExpandEast)
                    {
                        for (int i = 0; i < rightIndices.Count; i++)
                        {
                            int cellIndexToAdd = rightIndices[i] + 1;
                            region.cellIndices.Add(cellIndexToAdd);
                            cellGrid.Cells[cellIndexToAdd].regionID = regionID;
                            tempIndices.Add(cellIndexToAdd);
                        }

                        rightIndices = new List<int>(tempIndices);
                        tempIndices.Clear();

                        // The last index in the right indices would be the region's top-right corner, which needs to be added to the top indices.
                        topRightIndex = rightIndices[rightIndices.Count - 1];
                        topIndices.Add(topRightIndex);
                    }

                    canExpandNorth = CanExpandNorth(topIndices, regionElevation);
                    canExpandEast = false;
                    if (!canExpandNorth) // If North is expanded, CanExpandEast() will be called automatically.
                    {
                        canExpandEast = CanExpandEast(rightIndices, regionElevation);
                    }
                }

                Vector3 regionCenter;
                if (region.cellIndices.Count > 1) {
                    regionCenter = (cellGrid.Cells[region.CellIndex_BottomLeft].transform.position + cellGrid.Cells[topRightIndex].transform.position) / 2;
                } else {
                    regionCenter = cellGrid.Cells[region.cellIndices[0]].transform.position;
                }

                RectTransform regionUI = Instantiate<RectTransform>(regionUIPrefab);
                region.FinishSettingRegion(regionCenter, topIndices, rightIndices, regionUI, canvas);

                cellIndex++;
            }
        }
    }

    void CalculateAllRegionPortals()
    {
        for (int i = 0; i < Regions.Count; i++)
        {
            CalculateRegionPortals(Regions[i], Regions[i].rightIndices, Direction.E);
            CalculateRegionPortals(Regions[i], Regions[i].topIndices, Direction.N);
        }
    }

    void PrintRightIndices(Region region)
    {
        string debugString = "Region ID: " + region.id + " Right Indices: ";
        for (int i = 0; i < region.rightIndices.Count; i++)
        {
            debugString += region.rightIndices[i] + " ";
        }
        Debug.Log(debugString);
    }

    /// <summary> Calculates a region's North or East portals. Assignment is bi-directional, so a region's South & West neighbors will be accounted for. </summary>
    void CalculateRegionPortals(Region region, List<int> indices, Direction directionToNeighborRegions)
    {
        int previousRegionID = -1;
        Edge firstEdge = null;
        Edge lastEdge = null;

        for (int i = 0; i < indices.Count; i++)
        {
            Cell cell = cellGrid.Cells[indices[i]];
            Cell neighbor = cell.GetNeighbor(directionToNeighborRegions);
            if (neighbor == null) // If null, there are no neighbors in the given direction.
            {
                break;
            }

            if (neighbor.regionID != previousRegionID) // First encounter with new region.
            {
                if (firstEdge != null) // The portal with the previous neighboring region must be created.
                {
                    Vector3 portalPosition;
                    if (lastEdge != null) // There was at least two neighboring cells of the previous region.
                    {
                        portalPosition = (firstEdge.Position + lastEdge.Position) / 2f;
                    }
                    else // There was only one neighboring cell of the previous region.
                    {
                        portalPosition = firstEdge.Position;
                    }

                    Portal portal = new Portal(portalPosition, region, Regions[previousRegionID], firstEdge.EdgeType);
                    region.portals.Add(portal);
                    Regions[previousRegionID].portals.Add(portal);

                    lastEdge = null;
                }

                firstEdge = GetEdgeToNeighbor(cell, neighbor, directionToNeighborRegions);

                if (i == indices.Count - 1) // This is the last right indice and the region connection only stretches one cell.
                {
                    Portal portal = new Portal(firstEdge.Position, region, Regions[neighbor.regionID], firstEdge.EdgeType);
                    region.portals.Add(portal);
                    Regions[neighbor.regionID].portals.Add(portal);
                }

                previousRegionID = neighbor.regionID;
            }
            else // Same neighboring region as previously encountered.
            {
                lastEdge = GetEdgeToNeighbor(cell, neighbor, directionToNeighborRegions);

                if (i == indices.Count - 1) // This is the last right indice and the region connection stretches across multiple cells.
                {
                    Portal portal = new Portal((firstEdge.Position + lastEdge.Position)/2, region, Regions[neighbor.regionID], firstEdge.EdgeType);
                    region.portals.Add(portal);
                    Regions[neighbor.regionID].portals.Add(portal);
                }
            }
        }
    }

    /// <summary> CONVOLUTED method to find the edge between the a cell and its neighbor. Warning, can return null! </summary>
    Edge GetEdgeToNeighbor(Cell cell, Cell neighbor, Direction directionToNeighbor)
    {
        if (directionToNeighbor == Direction.E)
        {
            for (int i = 0; i < cell.node.Edges.Count; i++)
            {
                if (cell.node.Edges[i].DirectionFromNodeOne == Direction.W && cell.node.Edges[i].NodeOne == neighbor.node)
                {
                    return cell.node.Edges[i];
                }
            }
        }
        else if (directionToNeighbor == Direction.N)
        {
            for (int i = 0; i < cell.node.Edges.Count; i++)
            {
                if (cell.node.Edges[i].DirectionFromNodeOne == Direction.S && cell.node.Edges[i].NodeOne == neighbor.node)
                {
                    return cell.node.Edges[i];
                }
            }
        }

        return null;
    }

    bool CanExpandNorth(List<int> topIndices, int regionElevation)
    {
        // If not on the North border, there are North neighbors to check.
        if (cellGrid.Cells[topIndices[0]].Coordinates.Z < cellGrid.Rows - 1)
        {
            // For every cell in the region's current top, look if North neighbor does not belong to a region or does not have the target elevation.
            for (int i = 0; i < topIndices.Count; i++)
            {
                if (cellGrid.Cells[topIndices[i] + cellGrid.Columns].regionID != -1 || cellGrid.Cells[topIndices[i] + cellGrid.Columns].Elevation != regionElevation)
                {
                    return false;
                }
            }

            return true;
        }
        else // Otherwise, this is a border cell.
        {
            return false;
        }
    }

    bool CanExpandEast(List<int> rightIndices, int regionElevation)
    {
        // If not on the East border, there are East neighbors to check.
        if (cellGrid.Cells[rightIndices[0]].Coordinates.X < cellGrid.Columns - 1)
        {
            // For every cell in the region's current right, look if East neighbor does not belong to a region or does not have the target elevation.
            for (int i = 0; i < rightIndices.Count; i++)
            {
                if (cellGrid.Cells[rightIndices[i] + 1].regionID != -1 || cellGrid.Cells[rightIndices[i] + 1].Elevation != regionElevation)
                {
                    return false;
                }
            }

            return true;
        }
        else // Otherwise, this is a border cell.
        {
            return false;
        }
    }

    void DisplayRegions()
    {
        for (int i = 0; i < Regions.Count; i++)
        {



        }
    }

    public void ResetAllSearchData()
    {
        for (int i = 0; i < Regions.Count; i++)
        {
            Regions[i].ResetSearchData();
        }
    }

    [SerializeField] GameObject terrainColliderPrefab;

    void GenerateColliders()
    {
        if (collidersGameObject == null)
        {
            collidersGameObject = new GameObject("TerrainColliders");
        }

        float regionWidth;
        float regionLength;
        float regionHeight;

        for (int i = 0; i < Regions.Count; i++)
        {
            regionWidth = Regions[i].topIndices.Count * CellMetrics.offset;
            regionLength = Regions[i].rightIndices.Count * CellMetrics.offset;
            regionHeight = (cellGrid.Cells[Regions[i].rightIndices[0]].Elevation + 1) * CellMetrics.offset;

            GameObject terrainCollider = Instantiate(terrainColliderPrefab);
            numCollidersCreated++;
            terrainCollider.name = string.Format("TerrainCollider: Region #{0}", i.ToString());
            terrainCollider.transform.parent = collidersGameObject.transform;
            terrainCollider.transform.localScale = new Vector3(regionWidth, regionHeight, regionLength);
            terrainCollider.transform.position = new Vector3(Regions[i].centerPosition.x, (regionHeight * 0.5f) - 1, Regions[i].centerPosition.z);
        }
    }

    public int GetRegionID(Vector3 position)
    {
        return cellGrid.GetCell(position).regionID;
    }
}
