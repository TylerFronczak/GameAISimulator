//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. http://catlikecoding.com/unity/tutorials/
//*************************************************************************************************

using System.IO;
using UnityEngine;
using UnityEngine.UI;
using GameAISimulator.Enums;

public class Cell : MonoBehaviour
{
    public Coordinates Coordinates { get; private set; }

    public AtlasTexture baseTexture = AtlasTexture.Dirt0;
    public AtlasTexture topTexture = AtlasTexture.Grass0;

    public TopType topType = TopType.Flat;
    public Direction topDirection = Direction.None;

    public Node node;
    Cell[] neighbors;
    public Cell[] Neighbors { get { return neighbors; } }

    public int regionID = -1;

    public IGridObject obstacle;
    private IGridObject feature;
    public IGridObject Feature
    {
        get { return feature; }
        set
        {
            if (feature != null && value != null)
            {
                feature.Replace();
                Debug.LogWarning("The preexisting cell feature was replaced.");
            }

            feature = value;

            if (feature != null)
            {
                hasFeature = true;
            }
        }
    }

    public bool hasFeature;

    public void DeregisterAgent(Agent agent)
    {
        if (obstacle == agent as IGridObject)
        {
            obstacle = null;
        }
    }

    public void RegisterAgent(Agent agent)
    {
        //if (obstacle != null)
        //{
        //    Debug.LogWarning(string.Format("{0} has two obstacles, the original was overridden by {1}", gameObject.name, agent.agentName));
        //}

        obstacle = agent;
    }

    int elevation;
    public int Elevation
    {
        get {
            return elevation;
        }
        set {
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value;// * CellMetrics.offset;
            transform.localPosition = position;

            if (topType == TopType.Slope) {
                node.position = position + new Vector3(0, CellMetrics.radius, 0);
            } else {
                node.position = position;
            }

            Vector3 uiPosition = cellUI.localPosition;
            uiPosition.z = elevation * -CellMetrics.offset;
            cellUI.localPosition = uiPosition;
        }
    }

    public void Initialize(int column, int row, Transform parent)
    {
        neighbors = new Cell[8];
        transform.localPosition = new Vector3(column * CellMetrics.offset, 0f, row * CellMetrics.offset);
        transform.SetParent(parent, false);
        Coordinates = new Coordinates(column, row);
        this.name = "Cell" + Coordinates.ToString();
    }

    public Cell GetNeighbor(Direction direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(Direction direction, Cell neighbor)
    {
        //Debug.Log("Neighbors set: " + direction.ToString() + "!");
        neighbors[(int)direction] = neighbor;
        neighbor.neighbors[(int)direction.Opposite()] = this;
    }

    #region UI
    [SerializeField] RectTransform cellUI;
    [SerializeField] Image cellOutline;

    public void InitializeCellUI(RectTransform uiRect, Canvas canvas)
    {
        cellUI = uiRect;
        cellUI.SetParent(canvas.transform, false);
        cellUI.anchoredPosition = new Vector2(this.transform.position.x, this.transform.position.z);
        //label.text = cell.coordinates.ToString();
        cellUI.name = "CellUI" + Coordinates.ToString();
        cellOutline = cellUI.GetComponentInChildren<Image>();
        cellOutline.enabled = false;
    }

    public void ToggleOutline(bool isEnabled)
    {
        if (isEnabled) {
            cellOutline.enabled = true;
        } else {
            cellOutline.enabled = false;
        }
    }

    public void UpdateOutline(SearchStatus searchStatus)
    {
        switch (searchStatus)
        {
            case SearchStatus.None:
                cellOutline.color = Color.white;
                break;
            case SearchStatus.OpenList:
                cellOutline.color = Color.green;
                break;
            case SearchStatus.ClosedList:
                cellOutline.color = Color.red;
                break;
        }
    }
    #endregion

    public void Save(BinaryWriter binaryWriter)
    {
        binaryWriter.Write((byte)elevation);
        binaryWriter.Write((byte)baseTexture);
        binaryWriter.Write((byte)topTexture);
        binaryWriter.Write((byte)topType);
        binaryWriter.Write((byte)topDirection);
    }

    public void Load(BinaryReader binaryReader)
    {
        Elevation = binaryReader.ReadByte();
        baseTexture = (AtlasTexture)binaryReader.ReadByte();
        topTexture = (AtlasTexture)binaryReader.ReadByte();
        topType = (TopType)binaryReader.ReadByte();
        topDirection = (Direction)binaryReader.ReadByte();
    }
}

[System.Serializable]
public struct Coordinates
{
    [SerializeField] int x, z;

    public int X
    {
        get { return x; }
    }

    public int Z
    {
        get { return z; }
    }

    public Coordinates(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public override string ToString()
    {
        return "(" + x.ToString() + ", " + z.ToString() + ")";
    }
}
