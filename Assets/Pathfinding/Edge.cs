//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;
using GameAISimulator.Enums;

public class Edge
{
    public Vector3 Position { get; private set; }

    public EdgeType EdgeType { get; private set; }
    public float Cost { get; private set; }

    public Node NodeOne { get; private set; }
    public Node NodeTwo { get; private set; }

    public Direction DirectionFromNodeOne { get; private set; } // Only directions that need to be accounted for are W,S,SW,SE as per the grid construction pattern.

    public Edge(Node nodeOne, Node nodeTwo, Direction directionFromNodeOne)
    {
        NodeOne = nodeOne;
        NodeTwo = nodeTwo;

        DirectionFromNodeOne = directionFromNodeOne;
        Cost = DetermineCost(directionFromNodeOne);

        RecalculateEdgeData();
    }

    public Node OtherNode(Node node)
    {
        if (node == NodeOne)
        {
            return NodeTwo;
        }
        else if (node == NodeTwo)
        {
            return NodeOne;
        }
        else
        {
            Debug.LogWarning("Edge does not contain passed node.");
            return null;
        }
    }

    public void RecalculateEdgeData()
    {
        Cell cellOne = NodeOne.Cell;
        Cell cellTwo = NodeTwo.Cell;
        Vector3 averagePosition = (cellOne.transform.position + cellTwo.transform.position) / 2;
        int elevationDifference = Mathf.Abs(cellOne.Elevation - cellTwo.Elevation);

        if (cellOne.topType == TopType.Flat && cellTwo.topType == TopType.Flat)
        {
            if (DirectionFromNodeOne == Direction.SW || DirectionFromNodeOne == Direction.SE)
            {
                Cell southNeighbor;
                Cell eastOrWestNeighbor;

                if (DirectionFromNodeOne == Direction.SW)
                {
                    southNeighbor = cellOne.GetNeighbor(Direction.S);
                    eastOrWestNeighbor = cellOne.GetNeighbor(Direction.W);
                }
                else // Direction.SE
                {
                    southNeighbor = cellOne.GetNeighbor(Direction.S);
                    eastOrWestNeighbor = cellOne.GetNeighbor(Direction.E);
                }

                if (southNeighbor.topType == TopType.Flat && eastOrWestNeighbor.topType == TopType.Flat)
                {
                    int[] elevationsToCompare = new int[4];
                    elevationsToCompare[0] = cellOne.Elevation;
                    elevationsToCompare[1] = cellTwo.Elevation;
                    elevationsToCompare[2] = southNeighbor.Elevation;
                    elevationsToCompare[3] = eastOrWestNeighbor.Elevation;

                    if (AreElevationsVaried(elevationsToCompare))
                    {
                        EdgeType = EdgeType.Elevated;

                        Position = new Vector3(averagePosition.x, GetHighestElevation(elevationsToCompare), averagePosition.z);
                    }
                    else
                    {
                        EdgeType = EdgeType.Flat;
                        Position = new Vector3(averagePosition.x, cellOne.Elevation, averagePosition.z);
                    }
                }
                else if (eastOrWestNeighbor.topType == TopType.Slope || southNeighbor.topType == TopType.Slope)
                {
                    int southNeighborCornerElevation;
                    int eastOrWestNeighborCornerElevation;

                    if (eastOrWestNeighbor == cellOne.GetNeighbor(Direction.W)) // West neighbor
                    {
                        if (eastOrWestNeighbor.topType == TopType.Slope)
                        {
                            // If this West neighbor is a North or West facing slope, the relevant corner is along the slope bottom.
                            if (eastOrWestNeighbor.topDirection == Direction.N || eastOrWestNeighbor.topDirection == Direction.W)
                            {
                                eastOrWestNeighborCornerElevation = eastOrWestNeighbor.Elevation;
                            }
                            else // Otherwise, this is a East or South facing slope, with the relevant corner along the slope top.
                            {
                                eastOrWestNeighborCornerElevation = eastOrWestNeighbor.Elevation + 1;
                            }
                        }
                        else
                        {
                            eastOrWestNeighborCornerElevation = eastOrWestNeighbor.Elevation;
                        }

                        if (southNeighbor.topType == TopType.Slope)
                        {
                            // If this South neighbor is a East or South facing slope, the relevant corner is along the slope bottom.
                            if (southNeighbor.topDirection == Direction.E || southNeighbor.topDirection == Direction.S)
                            {
                                southNeighborCornerElevation = eastOrWestNeighbor.Elevation;
                            }
                            else // Otherwise, this is a North or West facing slope, with the relevant corner along the slope top.
                            {
                                southNeighborCornerElevation = eastOrWestNeighbor.Elevation + 1;
                            }
                        }
                        else
                        {
                            southNeighborCornerElevation = southNeighbor.Elevation;
                        }

                    }
                    else // East neighbor
                    {
                        if (eastOrWestNeighbor.topType == TopType.Slope)
                        {
                            // If this East neighbor is a North or East facing slope, the relevant corner is along the slope bottom.
                            if (eastOrWestNeighbor.topDirection == Direction.N || eastOrWestNeighbor.topDirection == Direction.E)
                            {
                                eastOrWestNeighborCornerElevation = eastOrWestNeighbor.Elevation;
                            }
                            else // Otherwise, this is a West or South facing slope, with the relevant corner along the slope top.
                            {
                                eastOrWestNeighborCornerElevation = eastOrWestNeighbor.Elevation + 1;
                            }
                        }
                        else
                        {
                            eastOrWestNeighborCornerElevation = eastOrWestNeighbor.Elevation;
                        }

                        if (southNeighbor.topType == TopType.Slope)
                        {
                            // If this South neighbor is a South or West facing slope, the relevant corner is along the slope bottom.
                            if (southNeighbor.topDirection == Direction.S || southNeighbor.topDirection == Direction.W)
                            {
                                southNeighborCornerElevation = eastOrWestNeighbor.Elevation;
                            }
                            else // Otherwise, this is a North or East facing slope, with the relevant corner along the slope top.
                            {
                                southNeighborCornerElevation = eastOrWestNeighbor.Elevation + 1;
                            }
                        }
                        else
                        {
                            southNeighborCornerElevation = southNeighbor.Elevation;
                        }
                    }

                    int[] elevationsToCompare = new int[4];
                    elevationsToCompare[0] = cellOne.Elevation;
                    elevationsToCompare[1] = cellTwo.Elevation;
                    elevationsToCompare[2] = southNeighborCornerElevation;
                    elevationsToCompare[3] = eastOrWestNeighborCornerElevation;

                    EdgeType = EdgeType.Inaccessible;
                    Position = new Vector3(averagePosition.x, GetHighestElevation(elevationsToCompare), averagePosition.z);
                }
            }
            else // Direction is West or South
            {
                if (elevationDifference != 0)
                {
                    EdgeType = EdgeType.Elevated;
                }
                else
                {
                    EdgeType = EdgeType.Flat;
                }

                float highestElevation = GetHighestElevation(cellOne.Elevation, cellTwo.Elevation);

                Position = new Vector3(averagePosition.x, highestElevation, averagePosition.z);
            }
        }
        else if (cellOne.topType == TopType.Slope && cellTwo.topType == TopType.Slope)
        {
            if (cellOne.topDirection == cellTwo.topDirection)
            {
                if (cellOne.topDirection == DirectionFromNodeOne || cellOne.topDirection == DirectionFromNodeOne.Opposite()) // Collinear, facing same direction
                {
                    if (elevationDifference == 0)
                    {
                        EdgeType = EdgeType.Elevated;
                        Position = averagePosition + new Vector3(0f, CellMetrics.offset, 0f);
                    }
                    else
                    {
                        // Assuming slope
                        if (elevationDifference == 1)
                        {
                            EdgeType = EdgeType.Slope;
                            Position = averagePosition + new Vector3(0f, CellMetrics.radius, 0f);
                        }
                        else
                        {
                            EdgeType = EdgeType.Elevated;
                            Position = averagePosition + new Vector3(0f, elevationDifference / 2, 0f);
                        }
                    }
                }
                else if (DirectionFromNodeOne == Direction.S || DirectionFromNodeOne == Direction.W) // Side by side, facing same direction
                {
                    Position = averagePosition + new Vector3(0f, elevationDifference / 2 + CellMetrics.radius, 0f);

                    if (elevationDifference == 0)
                    {
                        EdgeType = EdgeType.Slope;
                    }
                    else
                    {
                        EdgeType = EdgeType.Elevated;
                    }
                }
                else // Diagonal, facing same direction
                {
                    Position = averagePosition + new Vector3(0f, elevationDifference / 2, 0f);

                    if (elevationDifference == 1 && cellOne.Elevation > cellTwo.Elevation)
                    {
                        EdgeType = EdgeType.Slope;
                    }
                    else
                    {
                        EdgeType = EdgeType.Elevated;
                    }
                }
            }
            else if (cellOne.topDirection == cellTwo.topDirection.Opposite())
            {
                if (cellOne.topDirection == DirectionFromNodeOne || cellOne.topDirection == DirectionFromNodeOne.Opposite()) // Collinear, facing opposite directions
                {
                    Position = averagePosition + new Vector3(0f, elevationDifference / 2 + CellMetrics.offset, 0f);
                    if (elevationDifference == 0) // Edge point on peak
                    {
                        EdgeType = EdgeType.SlopeConnection;
                    }
                    else // Edge point on highest slope edge
                    {
                        EdgeType = EdgeType.Elevated;
                    }
                }
                else if (DirectionFromNodeOne == Direction.S || DirectionFromNodeOne == Direction.W) // Side by side, facing opposite directions 
                {
                    // Edge point on side of highest slope or between crossed triangles, both of which are deemed non-traversable
                    EdgeType = EdgeType.Inaccessible;
                    Position = averagePosition + new Vector3(0f, elevationDifference / 2 + CellMetrics.radius, 0f);
                }
                else // Diagonal positioned slopes, facing opposite directions
                {
                    Position = averagePosition + new Vector3(0f, elevationDifference / 2 + CellMetrics.offset, 0f);

                    if (elevationDifference == 0)
                    {
                        EdgeType = EdgeType.SlopeConnection;
                    }
                    else
                    {
                        EdgeType = EdgeType.Elevated;
                    }
                }
            }
        }
        else if (cellOne.topType == TopType.Slope && cellTwo.topType == TopType.Flat)
        {
            if (DirectionFromNodeOne == Direction.S || DirectionFromNodeOne == Direction.W)
            {
                if (cellOne.topDirection == DirectionFromNodeOne) // Slope is facing neighbor
                {
                    if (elevationDifference == 1 && cellOne.Elevation < cellTwo.Elevation)
                    {
                        EdgeType = EdgeType.SlopeConnection;
                    }
                    else
                    {
                        EdgeType = EdgeType.Elevated;
                    }

                    float highestElevation = GetHighestElevation(cellOne.Elevation + 1, cellTwo.Elevation);
                    Position = new Vector3(averagePosition.x, highestElevation, averagePosition.z);
                }
                else if (cellOne.topDirection == DirectionFromNodeOne.Opposite()) // Slope is facing away from neighbor
                {
                    if (elevationDifference == 0)
                    {
                        EdgeType = EdgeType.SlopeConnection;
                    }
                    else
                    {
                        EdgeType = EdgeType.Elevated;
                    }

                    float highestElevation = GetHighestElevation(cellOne.Elevation, cellTwo.Elevation);
                    Position = new Vector3(averagePosition.x, highestElevation, averagePosition.z);
                }
                else // Slope is facing perpendicular from neighbor
                {
                    EdgeType = EdgeType.Inaccessible;
                    float highestElevation = GetHighestElevation(cellOne.Elevation + 0.5f, cellTwo.Elevation);
                    Position = new Vector3(averagePosition.x, highestElevation, averagePosition.z);
                }
            }
            else if (DirectionFromNodeOne == Direction.SW)
            {
                Cell southNeighbor = cellOne.GetNeighbor(Direction.S);
                Cell westNeighbor = cellOne.GetNeighbor(Direction.W);

                if (southNeighbor.topType == TopType.Flat && westNeighbor.topType == TopType.Flat)
                {
                    int[] elevationsToCompare = new int[4];

                    if (cellOne.topDirection == Direction.N || cellOne.topDirection == Direction.E)
                    {
                        elevationsToCompare[0] = cellOne.Elevation;
                    }
                    else
                    {
                        elevationsToCompare[0] = cellOne.Elevation + 1;
                    }

                    elevationsToCompare[1] = cellTwo.Elevation;
                    elevationsToCompare[2] = southNeighbor.Elevation;
                    elevationsToCompare[3] = westNeighbor.Elevation;

                    EdgeType = EdgeType.Inaccessible;
                    Position = new Vector3(averagePosition.x, GetHighestElevation(elevationsToCompare), averagePosition.z);
                }
            }
            else // directionFromNodeOne == Direction.SE
            {
                Cell southNeighbor = cellOne.GetNeighbor(Direction.S);
                Cell eastNeighbor = cellOne.GetNeighbor(Direction.E);

                if (southNeighbor.topType == TopType.Flat && eastNeighbor.topType == TopType.Flat)
                {
                    int[] elevationsToCompare = new int[4];

                    if (cellOne.topDirection == Direction.N || cellOne.topDirection == Direction.W)
                    {
                        elevationsToCompare[0] = cellOne.Elevation;
                    }
                    else
                    {
                        elevationsToCompare[0] = cellOne.Elevation + 1;
                    }

                    elevationsToCompare[1] = cellTwo.Elevation;
                    elevationsToCompare[2] = southNeighbor.Elevation;
                    elevationsToCompare[3] = eastNeighbor.Elevation;

                    EdgeType = EdgeType.Inaccessible;
                    Position = new Vector3(averagePosition.x, GetHighestElevation(elevationsToCompare), averagePosition.z);
                }
            }
        }
        else if (cellOne.topType == TopType.Flat && cellTwo.topType == TopType.Slope)
        {
            if (DirectionFromNodeOne == Direction.S || DirectionFromNodeOne == Direction.W)
            {
                if (DirectionFromNodeOne == cellTwo.topDirection.Opposite()) // Slope pointing towards flat.
                {
                    if (elevationDifference == 1 && cellOne.Elevation > cellTwo.Elevation)
                    {
                        EdgeType = EdgeType.SlopeConnection;
                    }
                    else // Edge point on higher flat surface
                    {
                        EdgeType = EdgeType.Elevated;
                    }

                    float highestElevation = GetHighestElevation(cellOne.Elevation, cellTwo.Elevation + 1);
                    Position = new Vector3(averagePosition.x, highestElevation, averagePosition.z);
                }
                else if (DirectionFromNodeOne == cellTwo.topDirection) // Slope pointing opposite of flat.
                {
                    if (elevationDifference == 1 && cellOne.Elevation < cellTwo.Elevation)
                    {
                        EdgeType = EdgeType.SlopeConnection;
                    }
                    else if (elevationDifference == 0)
                    {
                        EdgeType = EdgeType.SlopeConnection;
                    }
                    else // Edge point on higher flat surface
                    {
                        EdgeType = EdgeType.Elevated;
                    }

                    float highestElevation = GetHighestElevation(cellOne.Elevation, cellTwo.Elevation);
                    Position = new Vector3(averagePosition.x, highestElevation, averagePosition.z);
                }
                else // Slope pointing perpendicular
                {
                    EdgeType = EdgeType.Inaccessible;
                    float highestElevation = GetHighestElevation(cellOne.Elevation, cellTwo.Elevation + 0.5f);
                    Position = new Vector3(averagePosition.x, highestElevation, averagePosition.z);
                }
            }
            else if (DirectionFromNodeOne == Direction.SE)
            {

            }
            else if (DirectionFromNodeOne == Direction.SW)
            {

            }
        }
    }

    bool AreElevationsVaried(int[] elevations)
    {
        bool isVaried = false;
        for (int i = 0; i < elevations.Length; i++)
        {
            if (elevations[0] != elevations[i])
            {
                isVaried = true;
                break;
            }
        }

        return isVaried;
    }
    float GetHighestElevation(int[] elevations)
    {
        return Mathf.Max(elevations) * CellMetrics.offset;
    }
    float GetHighestElevation(float valueOne, float valueTwo)
    {
        return Mathf.Max(valueOne, valueTwo) * CellMetrics.offset;
    }

    float DetermineCost(Direction direction)
    {
        switch (direction)
        {
            case Direction.SE:
                return 1.41f;
            case Direction.S:
                return 1f;
            case Direction.SW:
                return 1.4f;
            case Direction.W:
                return 1f;
            default:
                return 1f;
        }
    }
}

public enum EdgeType
{
    Flat,
    Slope,
    SlopeConnection,
    Elevated,
    Inaccessible,
    None
}
