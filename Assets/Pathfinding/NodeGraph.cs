//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using GameAISimulator.Enums;

public class NodeGraph : MonoBehaviour
{
    public List<Node> Nodes { get; private set; }

    public void Initialize(Cell[] cells, int columns, int rows)
    {
        Nodes = new List<Node>();
        CreateAllNodes(cells);
        CreateAllNodeEdges(cells, columns, rows);
    }

    void CreateAllNodes(Cell[] cells)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].node = CreateNode(cells[i]);
        }
    }
    Node CreateNode(Cell cell)
    {
        Node node = new Node(cell);
        Nodes.Add(node);
        return node;
    }

    void CreateAllNodeEdges(Cell[] cells, int columns, int rows)
    {
        for (int i = 0, x = 0; x < rows; x++)
        {
            for (int z = 0; z < columns; z++)
            {
                CreateNodeEdges(cells, x, z, i++, columns);
            }
        }
    }
    void CreateNodeEdges(Cell[] cells, int row, int column, int index, int numColumns)
    {
        if (column > 0) // If not on West border, there is a West edge.
        {
            CreateEdge(Nodes[index], Nodes[index - 1], Direction.W);
        }

        if (row > 0) // If not on South border, there is a South edge.
        {
            CreateEdge(Nodes[index], Nodes[index - numColumns], Direction.S);

            if (column > 0) // If not on the West border, there is a SouthWest edge.
            {
                CreateEdge(Nodes[index], Nodes[index - (numColumns + 1)], Direction.SW);
            }

            if (column < numColumns - 1) // If not on the East border, there is a SouthEast edge.
            {
                CreateEdge(Nodes[index], Nodes[index - (numColumns - 1)], Direction.SE);
            }
        }
    }
    void CreateEdge(Node nodeOne, Node nodeTwo, Direction direction)
    {
        Edge edge = new Edge(nodeOne, nodeTwo, direction);
        nodeOne.Edges.Add(edge);
        nodeTwo.Edges.Add(edge);
    }

    public void ResetAllSearchData()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            Nodes[i].ResetSearchData();
        }
    }

    public void RecalculateAllNodeEdges()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            Nodes[i].RecalculateAllEdgeData();
        }
    }
}
