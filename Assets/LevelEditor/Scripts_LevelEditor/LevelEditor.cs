//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References:
//  1. https://answers.unity.com/questions/1115464/ispointerovergameobject-not-working-with-touch-inp.html
//  1. http://catlikecoding.com/unity/tutorials/
//
// TODO:
//  1. Editing with different brush sizes
//  2. Placing objects
//*************************************************************************************************

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GameAISimulator.Enums;

public class LevelEditor : MonoBehaviour
{
    public CellGrid cellGrid;
    public NodeGraph nodeGraph;
    public CustomTerrain customTerrain;

    AtlasTexture selectedTopTexture;
    bool isEditingTopTexture = true;

    AtlasTexture selectedSideTexture;
    bool isEditingSideTexture = true;

    bool isSelectingTopTexture;
    bool isSelectingSideTexture;
    [SerializeField] RawImage topTextureRawImage;
    [SerializeField] RawImage sideTextureRawImage;
    [SerializeField] GameObject textureOptions;

    int selectedElevation;
    bool isEditingElevation = true;

    TopType selectedTopType;
    bool isEditingTopType = true;

    Direction selectedTopDirection;
    bool isEditingTopDirection = true;

    [SerializeField] Carousel topTypeCarousel;
    [SerializeField] Carousel topDirectionCarousel;

    [SerializeField] GameObject levelEditorPanel;
    bool isLevelEditorEnabled = false;

    [SerializeField] AudioManager audioManager;

    void Start()
    {
        levelEditorPanel.SetActive(false);

        SetTopTexture(AtlasTexture.Grass0);
        SetBaseTexture(AtlasTexture.Dirt0);
        textureOptions.SetActive(false);

        Cell[] cells = cellGrid.Cells;

        RefreshMesh();
    }

    bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && !IsPointerOverUIObject())
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        if (isLevelEditorEnabled)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                EditCell(cellGrid.GetCell(hit.point));
            }
        }
    }

    void EditCell(Cell cell)
    {
        bool isCellChanged = false;

        if (isEditingTopTexture)
        {
            if (cell.topTexture != selectedTopTexture)
            {
                cell.topTexture = selectedTopTexture;
                isCellChanged = true;
            }
        }

        if (isEditingSideTexture)
        {
            if (cell.baseTexture != selectedSideTexture)
            {
                cell.baseTexture = selectedSideTexture;
                isCellChanged = true;
            }
        }

        if (isEditingElevation)
        {
            if (cell.Elevation != selectedElevation)
            {
                cell.Elevation = selectedElevation;
                isCellChanged = true;
            }
            else
            {
                if (cell.topType == TopType.Slope)
                {
                    cell.Elevation = selectedElevation;
                    isCellChanged = true;
                }
            }
        }

        if (isEditingTopType)
        {
            if (cell.topType != selectedTopType)
            {
                cell.topType = selectedTopType;
                isCellChanged = true;
            }
        }

        if (isEditingTopDirection)
        {
            if (cell.topDirection != selectedTopDirection)
            {
                cell.topDirection = selectedTopDirection;
                isCellChanged = true;
            }
        }

        if (isCellChanged)
        {
            RefreshMesh();
            nodeGraph.RecalculateAllNodeEdges();
            audioManager.PlayClip_CellEdit();
        }
    }

    void RefreshMesh()
    {
        Debug.Log("RefreshMesh()");
        customTerrain.Triangulate(cellGrid.Cells);
    }

    public void ClickedTopTextureSelector()
    {
        isSelectingTopTexture = !isSelectingTopTexture;
        isSelectingSideTexture = false;

        if (isSelectingTopTexture) {
            textureOptions.SetActive(true);
        } else {
            textureOptions.SetActive(false);
        }
    }

    public void ClickedSideTextureSelector()
    {
        isSelectingSideTexture = !isSelectingSideTexture;
        isSelectingTopTexture = false;

        if (isSelectingSideTexture) {
            textureOptions.SetActive(true);
        } else {
            textureOptions.SetActive(false);
        }
    }

    public void SelectTexture(int atlasTexture)
    {
        if (isSelectingTopTexture)
        {
            SetTopTexture((AtlasTexture)atlasTexture);
            isSelectingTopTexture = false;
        }
        else if (isSelectingSideTexture)
        {
            SetBaseTexture((AtlasTexture)atlasTexture);
            isSelectingSideTexture = false;
        }

        textureOptions.SetActive(false);
    }

    public void SetTopTexture(AtlasTexture atlasTexture)
    {
        selectedTopTexture = atlasTexture;
        UVData uvData = customTerrain.GetUVDataFor(selectedTopTexture);
        float width = uvData.uMax - uvData.uMin;
        float height = uvData.vMax - uvData.vMin;
        topTextureRawImage.uvRect = new Rect(uvData.uMin, uvData.vMin, width, height);
    }
    public void Toggle_IsEditingTopTexture(bool b)
    {
        isEditingTopTexture = b;
    }

    public void SetBaseTexture(AtlasTexture atlasTexture)
    {
        selectedSideTexture = atlasTexture;
        UVData uvData = customTerrain.GetUVDataFor(selectedSideTexture);
        float width = uvData.uMax - uvData.uMin;
        float height = uvData.vMax - uvData.vMin;
        sideTextureRawImage.uvRect = new Rect(uvData.uMin, uvData.vMin, width, height);
    }
    public void Toggle_IsEditingBaseTexture(bool b)
    {
        isEditingSideTexture = b;
    }

    public void SetElevation(float elevation)
    {
        selectedElevation = (int)elevation;
    }
    public void Toggle_IsEditingElevation(bool b)
    {
        isEditingElevation = b;
    }

    /// <summary> Called from UI carousel. </summary>
    public void SetTopType()
    {
        selectedTopType = (TopType)topTypeCarousel.GetIndex();
    }
    public void SetTopType(int topType)
    {
        selectedTopType = (TopType)topType;
    }
    public void Toggle_IsEditingTopType(bool b)
    {
        isEditingTopType = b;
    }

    /// <summary> Called from UI carousel. </summary>
    public void SetTopDirection()
    {
        selectedTopDirection = (Direction)topDirectionCarousel.GetIndex();
    }
    public void SetTopDirection(int direction)
    {
        selectedTopDirection = (Direction)direction;
    }
    public void Toggle_IsEditingTopDirection(bool b)
    {
        isEditingTopDirection = b;
    }

    public void Toggle_LevelEditorPanel()
    {
        isLevelEditorEnabled = !isLevelEditorEnabled;
        levelEditorPanel.SetActive(isLevelEditorEnabled);
    }

    #region Saving/Loading
    //string fileName = "TestMap";
    string fileType = ".bytes"; // Allows for recognition as a TextAsset

    // Note: Saves to the resources folder will only be recognized upon exiting playmode
    // and waiting a sufficient amount of time or forcing an update with Show In Explorer.
    public void Save(string fileName)
    {
        string filePath;

#if UNITY_EDITOR
        filePath = Application.dataPath + "/Resources/" + fileName + fileType;
        Debug.Log("UnityEditor");
#elif UNITY_ANDROID
        filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName + fileType);
        Debug.Log("UnityAndroid");
#endif

        using (
              BinaryWriter binaryWriter = new BinaryWriter(File.Open(filePath, FileMode.Create))
            )
        {
            binaryWriter.Write(0); // Save format, which may be used in the future for backwards compatibility.
            cellGrid.Save(binaryWriter);
        }
    }

    public void Load(string fileName, bool isUserCreated)
    {
        if (isUserCreated)
        {
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName + fileType);

            using (
                BinaryReader binaryReader = new BinaryReader(File.OpenRead(filePath))
                )
            {
                int saveFormat = binaryReader.ReadInt32();
                if (saveFormat == 0) {
                    cellGrid.Load(binaryReader);
                } else {
                    Debug.LogWarning("Unrecognized save format: " + saveFormat);
                }
            }
        }
        else // Packaged in build
        {
            TextAsset textAsset = Resources.Load(fileName) as TextAsset;
            Stream stream = new MemoryStream(textAsset.bytes);

            using (
                BinaryReader binaryReader = new BinaryReader(stream)
                )
            {
                int saveFormat = binaryReader.ReadInt32();
                if (saveFormat == 0) {
                    cellGrid.Load(binaryReader);
                } else {
                    Debug.LogWarning("Unrecognized save format: " + saveFormat);
                }
            }
        }

        RefreshMesh();
        nodeGraph.RecalculateAllNodeEdges();
    }
    #endregion
}
