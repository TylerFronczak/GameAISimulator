//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

public interface IGridObject
{
    GridObjectType Type { get; }

    CellGrid CellGrid { get; set; }
    Cell Cell { get; set; }

    bool PlaceOnGrid(int row, int column);
    bool PlaceOnGrid(Cell cell);
    void RemoveFromGrid();
    void Replace();
    void OnDestroy(); // Should call RemoveFromGrid()
}

public enum GridObjectType
{
    Agent,
    Food
}
