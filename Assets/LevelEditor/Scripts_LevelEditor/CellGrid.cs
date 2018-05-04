//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. http://catlikecoding.com/unity/tutorials/
//*************************************************************************************************

using System.IO;
using UnityEngine;
using GameAISimulator.Enums;

public class CellGrid : MonoBehaviour
{
    public int Columns { get; private set; }
    public int Rows { get; private set; }

    public Cell[] Cells { get; private set; }
    public NodeGraph NodeGraph { get; private set; }

    [SerializeField] Cell cellPrefab;
    [SerializeField] RectTransform cellUIPrefab;

    public void Initialize(int columns, int rows, Canvas canvas)
    {
        Columns = columns;
        Rows = rows;

        CreateAllCells(canvas);
        SetAllNeighbors();
    }

    void CreateAllCells(Canvas canvas)
    {
        Cells = new Cell[Rows * Columns];

        for (int i = 0, x = 0; x < Rows; x++)
        {
            for (int z = 0; z < Columns; z++)
            {
                CreateCell(x, z, i++, canvas);
            }
        }
    }
    void CreateCell(int row, int column, int index, Canvas canvas)
    {
        Cell cell = Cells[index] = Instantiate<Cell>(cellPrefab);
        cell.Initialize(column, row, this.transform);

        RectTransform cellUI = Instantiate<RectTransform>(cellUIPrefab);
        cell.InitializeCellUI(cellUI, canvas);
    }

    void SetAllNeighbors()
    {
        for (int i = 0, x = 0; x < Rows; x++)
        {
            for (int z = 0; z < Columns; z++)
            {
                SetNeighbors(x, z, i++);
            }
        }
    }
    void SetNeighbors(int row, int column, int index)
    {
        if (column > 0) // If not on West border, there is a West neighbor.
        {
            Cells[index].SetNeighbor(Direction.W, Cells[index - 1]);
        }

        if (row > 0) // If not on South border, there is a South neighbor.
        {
            Cells[index].SetNeighbor(Direction.S, Cells[index - Columns]);

            if (column > 0) // If not on the West border, there is a SouthWest neighbor.
            {
                Cells[index].SetNeighbor(Direction.SW, Cells[index - (Columns + 1)]);
            }

            if (column < Columns - 1) // If not on the East border, there is a SouthEast neighbor.
            {
                Cells[index].SetNeighbor(Direction.SE, Cells[index - (Columns - 1)]);
            }
        }
    }

    public Cell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        Coordinates coordinates = FromPosition(position);
        int index = coordinates.X + coordinates.Z * Columns;
        return Cells[index];
    }

    public Cell GetCell(int column, int row)
    {
        return Cells[column + row * Columns];
    }

    public Cell GetRandomCell(int elevation)
    {
        int randomIndex = Random.Range(0, Cells.Length);
        int originalIndex = randomIndex;
        Cell cell = Cells[randomIndex];

        while (Cells[randomIndex].Elevation != elevation)
        {
            randomIndex++;
            if (randomIndex == originalIndex)
            {
                Debug.LogError("A random cell of the desired elevation could not be found.");
                return null;
            }

            if (randomIndex == Cells.Length)
            {
                randomIndex = 0;
            }
        }

        return Cells[randomIndex];
    }

    public Cell GetRandomCell_ObstacleFree(int elevation)
    {
        int randomIndex = Random.Range(0, Cells.Length);
        int originalIndex = randomIndex;
        Cell cell = Cells[randomIndex];

        while (Cells[randomIndex].Elevation != elevation || Cells[randomIndex].obstacle != null)
        {
            randomIndex++;
            if (randomIndex == originalIndex)
            {
                Debug.LogError("A random cell of the desired elevation could not be found.");
                return null;
            }

            if (randomIndex == Cells.Length)
            {
                randomIndex = 0;
            }
        }

        return Cells[randomIndex];
    }

    public Coordinates FromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / (CellMetrics.radius * 2f));
        x = Mathf.Clamp(x, 0, Columns - 1);

        int z = Mathf.RoundToInt(position.z / (CellMetrics.radius * 2f));
        z = Mathf.Clamp(z, 0, Rows - 1);

        return new Coordinates(x,z);
    }

    public void Save(BinaryWriter binaryWriter)
    {
        for (int i = 0; i < Cells.Length; i++)
        {
            Cells[i].Save(binaryWriter);
        }
    }

    public void Load(BinaryReader binaryReader)
    {
        for (int i = 0; i < Cells.Length; i++)
        {
            Cells[i].Load(binaryReader);
        }
    }
}
