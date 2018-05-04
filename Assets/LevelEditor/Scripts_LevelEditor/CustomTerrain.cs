//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. Catlike Coding
//  2. https://www.youtube.com/watch?v=v9E47DkckBE
//  3. https://www.youtube.com/watch?v=8LTDFwWMlqQ
//  4. https://www.youtube.com/watch?v=EWQpo4sjuxw
//  5. https://docs.unity3d.com/ScriptReference/Mesh.html
//
// Notes: 
//  1. Unity Docs- "The baseVertex argument (of Mesh.SetTriangles()) can be used to achieve 
//     meshes that are larger than 65535 vertices while using 16 bit index buffers, as long
//     as each submesh fits within its own 65535 vertex area.
//
// TODO: 
//  1. Refactor this into a managable size.
//  2. Allow editing chunks, rather than entire mesh.
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using GameAISimulator.Enums;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CustomTerrain : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;
    List<Vector2> uvs;

    MeshCollider meshCollider;

    const float texturePixelSize = 128;
    const float atlasPixelSize = 1024;
    const float textureReadOffset = 0.01f;
    float portionOfAtlas = texturePixelSize / atlasPixelSize;

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        mesh.name = "Grid Mesh";
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();
    }

    public void Triangulate(Cell[] cells)
    {
        mesh.Clear();
        vertices.Clear();
        uvs.Clear();
        triangles.Clear();

        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }

        mesh.SetVertices(vertices); //Said to be faster than mesh.vertices = vertices.ToArray()
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = mesh;
    }

    void Triangulate(Cell cell)
    {
        //CAUTION: Always check if neighbor is NULL before using
        Cell southNeighbor = cell.GetNeighbor(Direction.S);
        Cell northNeighbor = cell.GetNeighbor(Direction.N);
        Cell eastNeighbor = cell.GetNeighbor(Direction.E);
        Cell westNeighbor = cell.GetNeighbor(Direction.W);

        //Tops

        Vector3 center = cell.transform.localPosition;
        if (cell.topType == TopType.Flat)
        {
            AddQuad(
                center + CellMetrics.corners[3],
                center + CellMetrics.corners[0],
                center + CellMetrics.corners[1],
                center + CellMetrics.corners[2],
                cell.topTexture
            );
        }
        else if (cell.topType == TopType.Slope)
        {
            bool isNorthFaceVisible = true;
            bool isEastFaceVisible = true;
            bool isSouthFaceVisible = true;
            bool isWestFaceVisible = true;

            Vector3 centerTop = center + new Vector3(0, CellMetrics.offset, 0);
            if (cell.topDirection == Direction.N)
            {
                AddQuad(
                    center + CellMetrics.corners[3],
                    centerTop + CellMetrics.corners[0],
                    centerTop + CellMetrics.corners[1],
                    center + CellMetrics.corners[2],
                    cell.topTexture
                );

                //West face

                if (westNeighbor == null) {
                    isWestFaceVisible = true;
                } else if (westNeighbor.Elevation < cell.Elevation) {
                    isWestFaceVisible = true;
                } else if (westNeighbor.Elevation > cell.Elevation) {
                    isWestFaceVisible = false;
                }else if (westNeighbor.topType != TopType.Slope) {
                    isWestFaceVisible = true;
                } else if (westNeighbor.topDirection == Direction.E || westNeighbor.topDirection == Direction.N) {
                    isWestFaceVisible = false;
                }

                if (isWestFaceVisible)
                {
                    AddTriangle(
                    center + CellMetrics.corners[0],
                    centerTop + CellMetrics.corners[0],
                    center + CellMetrics.corners[3],
                    cell.baseTexture,
                    RelativeDirection.LL
                    );
                }

                //North face

                if (northNeighbor == null) {
                    isNorthFaceVisible = true;
                } else if (northNeighbor.Elevation < cell.Elevation) {
                    isNorthFaceVisible = true;
                } else if (northNeighbor.Elevation > cell.Elevation) {
                    isNorthFaceVisible = false;
                }else if (northNeighbor.topType != TopType.Slope) {
                    isNorthFaceVisible = true;
                } else if (northNeighbor.topDirection == Direction.S) {
                    isNorthFaceVisible = false;
                }

                if (isNorthFaceVisible)
                {
                    AddQuad(
                        center + CellMetrics.corners[1],
                        centerTop + CellMetrics.corners[1],
                        centerTop + CellMetrics.corners[0],
                        center + CellMetrics.corners[0],
                        cell.baseTexture
                    );
                }

                //East face

                if (eastNeighbor == null) {
                    isEastFaceVisible = true;
                } else if (eastNeighbor.Elevation < cell.Elevation) {
                    isEastFaceVisible = true;
                } else if (eastNeighbor.Elevation > cell.Elevation) {
                    isEastFaceVisible = false;
                }else if (eastNeighbor.topType != TopType.Slope) {
                    isEastFaceVisible = true;
                } else if (eastNeighbor.topDirection == Direction.W || eastNeighbor.topDirection == Direction.N) {
                    isEastFaceVisible = false;
                }

                if (isEastFaceVisible)
                {
                    AddTriangle(
                        center + CellMetrics.corners[2],
                        centerTop + CellMetrics.corners[1],
                        center + CellMetrics.corners[1],
                        cell.baseTexture,
                        RelativeDirection.LR
                    );
                }
            }
            else if (cell.topDirection == Direction.E)
            {
                AddQuad(
                    center + CellMetrics.corners[0],
                    centerTop + CellMetrics.corners[1],
                    centerTop + CellMetrics.corners[2],
                    center + CellMetrics.corners[3],
                    cell.topTexture
                );

                //North face

                if (northNeighbor == null) {
                    isNorthFaceVisible = true;
                } else if (northNeighbor.Elevation < cell.Elevation) {
                    isNorthFaceVisible = true;
                } else if (northNeighbor.Elevation > cell.Elevation) {
                    isNorthFaceVisible = false;
                }else if (northNeighbor.topType != TopType.Slope) {
                    isNorthFaceVisible = true;
                } else if (northNeighbor.topDirection == cell.topDirection || northNeighbor.topDirection == Direction.S) {
                    isNorthFaceVisible = false;
                }

                if (isNorthFaceVisible)
                {
                    AddTriangle(
                        center + CellMetrics.corners[1],
                        centerTop + CellMetrics.corners[1],
                        center + CellMetrics.corners[0],
                        cell.baseTexture,
                        RelativeDirection.LL
                    );
                }

                //East face

                if (eastNeighbor == null) {
                    isEastFaceVisible = true;
                } else if (eastNeighbor.Elevation < cell.Elevation) {
                    isEastFaceVisible = true;
                } else if (eastNeighbor.Elevation > cell.Elevation) {
                    isEastFaceVisible = false;
                }else if (eastNeighbor.topType != TopType.Slope) {
                    isEastFaceVisible = true;
                } else if (eastNeighbor.topDirection == cell.topDirection.Opposite()) {
                    isEastFaceVisible = false;
                }

                if (isEastFaceVisible)
                {
                    AddQuad(
                        center + CellMetrics.corners[2],
                        centerTop + CellMetrics.corners[2],
                        centerTop + CellMetrics.corners[1],
                        center + CellMetrics.corners[1],
                        cell.baseTexture
                    );
                }

                //South face

                if (southNeighbor == null) {
                    isSouthFaceVisible = true;
                } else if (southNeighbor.Elevation < cell.Elevation) {
                    isSouthFaceVisible = true;
                } else if (southNeighbor.Elevation > cell.Elevation) {
                    isSouthFaceVisible = false;
                }else if (southNeighbor.topType != TopType.Slope) {
                    isSouthFaceVisible = true;
                } else if (southNeighbor.topDirection == cell.topDirection || southNeighbor.topDirection == Direction.N) {
                    isSouthFaceVisible = false;
                }

                if (isSouthFaceVisible)
                {
                    AddTriangle(
                        center + CellMetrics.corners[3],
                        centerTop + CellMetrics.corners[2],
                        center + CellMetrics.corners[2],
                        cell.baseTexture,
                        RelativeDirection.LR
                    );
                }
            }
            else if (cell.topDirection == Direction.S)
            {
                //North face

                AddQuad(
                    center + CellMetrics.corners[1],
                    centerTop + CellMetrics.corners[2],
                    centerTop + CellMetrics.corners[3],
                    center + CellMetrics.corners[0],
                    cell.topTexture
                );

                //East face

                if (eastNeighbor == null) {
                    isEastFaceVisible = true;
                } else if (eastNeighbor.Elevation < cell.Elevation) {
                    isEastFaceVisible = true;
                } else if (eastNeighbor.Elevation > cell.Elevation) {
                    isEastFaceVisible = false;
                }else if (eastNeighbor.topType != TopType.Slope) {
                    isEastFaceVisible = true;
                } else if (eastNeighbor.topDirection == cell.topDirection || eastNeighbor.topDirection == Direction.W) {
                    isEastFaceVisible = false;
                }

                if (isEastFaceVisible)
                {
                    AddTriangle(
                        center + CellMetrics.corners[2],
                        centerTop + CellMetrics.corners[2],
                        center + CellMetrics.corners[1],
                        cell.baseTexture,
                        RelativeDirection.LL
                    );
                }

                //South face

                if (southNeighbor == null) {
                    isSouthFaceVisible = true;
                } else if (southNeighbor.Elevation < cell.Elevation) {
                    isSouthFaceVisible = true;
                } else if (southNeighbor.Elevation > cell.Elevation) {
                    isSouthFaceVisible = false;
                }else if (southNeighbor.topType != TopType.Slope) {
                    isSouthFaceVisible = true;
                } else if (southNeighbor.topDirection == cell.topDirection.Opposite()) {
                    isSouthFaceVisible = false;
                }

                if (isSouthFaceVisible)
                {
                    AddQuad(
                        center + CellMetrics.corners[3],
                        centerTop + CellMetrics.corners[3],
                        centerTop + CellMetrics.corners[2],
                        center + CellMetrics.corners[2],
                        cell.baseTexture
                    );
                }

                //West face

                if (westNeighbor == null) {
                    isWestFaceVisible = true;
                } else if (westNeighbor.Elevation < cell.Elevation) {
                    isWestFaceVisible = true;
                } else if (westNeighbor.Elevation > cell.Elevation) {
                    isWestFaceVisible = false;
                }else if (westNeighbor.topType != TopType.Slope) {
                    isWestFaceVisible = true;
                } else if (westNeighbor.topDirection == cell.topDirection || westNeighbor.topDirection == Direction.E) {
                    isWestFaceVisible = false;
                }

                if (isWestFaceVisible)
                {
                    AddTriangle(
                        center + CellMetrics.corners[0],
                        centerTop + CellMetrics.corners[3],
                        center + CellMetrics.corners[3],
                        cell.baseTexture,
                        RelativeDirection.LR
                    );
                }
            }
            else if (cell.topDirection == Direction.W)
            {
                //East face

                AddQuad(
                    center + CellMetrics.corners[2],
                    centerTop + CellMetrics.corners[3],
                    centerTop + CellMetrics.corners[0],
                    center + CellMetrics.corners[1],
                    cell.topTexture
                );

                //South face

                if (southNeighbor == null) {
                    isSouthFaceVisible = true;
                } else if (southNeighbor.Elevation < cell.Elevation) {
                    isSouthFaceVisible = true;
                } else if (southNeighbor.Elevation > cell.Elevation) {
                    isSouthFaceVisible = false;
                }else if (southNeighbor.topType != TopType.Slope) {
                    isSouthFaceVisible = true;
                } else if (southNeighbor.topDirection == cell.topDirection || southNeighbor.topDirection == Direction.N) {
                    isSouthFaceVisible = false;
                }

                if (isSouthFaceVisible)
                {
                    AddTriangle(
                        center + CellMetrics.corners[3],
                        centerTop + CellMetrics.corners[3],
                        center + CellMetrics.corners[2],
                        cell.baseTexture,
                        RelativeDirection.LL
                    );
                }

                //West face

                if (westNeighbor == null) {
                    isWestFaceVisible = true;
                } else if (westNeighbor.Elevation < cell.Elevation) {
                    isWestFaceVisible = true;
                } else if (westNeighbor.Elevation > cell.Elevation) {
                    isWestFaceVisible = false;
                }else if (westNeighbor.topType != TopType.Slope) {
                    isWestFaceVisible = true;
                } else if (westNeighbor.topDirection == cell.topDirection.Opposite()) {
                    isWestFaceVisible = false;
                }

                if (isWestFaceVisible)
                {
                    AddQuad(
                        center + CellMetrics.corners[0],
                        centerTop + CellMetrics.corners[0],
                        centerTop + CellMetrics.corners[3],
                        center + CellMetrics.corners[3],
                        cell.baseTexture
                    );
                }

                //North face

                if (northNeighbor == null) {
                    isNorthFaceVisible = true;
                } else if (northNeighbor.Elevation < cell.Elevation) {
                    isNorthFaceVisible = true;
                } else if (northNeighbor.Elevation > cell.Elevation) {
                    isNorthFaceVisible = false;
                }else if (northNeighbor.topType != TopType.Slope) {
                    isNorthFaceVisible = true;
                } else if (northNeighbor.topDirection == cell.topDirection || northNeighbor.topDirection == Direction.S) {
                    isNorthFaceVisible = false;
                }

                if (isNorthFaceVisible)
                {
                    AddTriangle(
                        center + CellMetrics.corners[1],
                        centerTop + CellMetrics.corners[0],
                        center + CellMetrics.corners[0],
                        cell.baseTexture,
                        RelativeDirection.LR
                    );
                }
            }
        }
        else if (cell.topType == TopType.Corner)
        {
            bool isNorthFaceVisible = true;
            bool isEastFaceVisible = true;
            bool isSouthFaceVisible = true;
            bool isWestFaceVisible = true;

            Vector3 centerTop = center + new Vector3(0, CellMetrics.offset, 0);

            if (cell.topDirection == Direction.NE)
            {
                //West face

                AddTriangle(
                    center + CellMetrics.corners[0],
                    centerTop + CellMetrics.corners[1],
                    center + CellMetrics.corners[3],
                    cell.topTexture,
                    RelativeDirection.LL
                );

                //South face

                AddTriangle(
                    center + CellMetrics.corners[3],
                    centerTop + CellMetrics.corners[1],
                    center + CellMetrics.corners[2],
                    cell.topTexture,
                    RelativeDirection.LR
                );

                //North face

                if (northNeighbor == null) {
                    isNorthFaceVisible = true;
                } else if (northNeighbor.Elevation < cell.Elevation) {
                    isNorthFaceVisible = true;
                } else if (northNeighbor.Elevation > cell.Elevation) {
                    isNorthFaceVisible = false;
                }else if (northNeighbor.topType != TopType.Slope && northNeighbor.topType != TopType.Corner) {
                    isNorthFaceVisible = true;
                } else if (northNeighbor.topType == TopType.Slope && (northNeighbor.topDirection == Direction.S || northNeighbor.topDirection == Direction.E)) {
                    isNorthFaceVisible = false;
                } else if (northNeighbor.topType == TopType.Corner && northNeighbor.topDirection == Direction.SE) {
                    isNorthFaceVisible = false;
                }

                if (isNorthFaceVisible)
                {
                    AddTriangle(
                        center + CellMetrics.corners[1],
                        centerTop + CellMetrics.corners[1],
                        center + CellMetrics.corners[0],
                        cell.baseTexture,
                        RelativeDirection.LL
                    );
                }

                // East face

                if (eastNeighbor == null) {
                    isEastFaceVisible = true;
                } else if (eastNeighbor.Elevation < cell.Elevation) {
                    isEastFaceVisible = true;
                } else if (eastNeighbor.Elevation > cell.Elevation) {
                    isEastFaceVisible = false;
                }else if (eastNeighbor.topType != TopType.Slope && eastNeighbor.topType != TopType.Corner) {
                    isEastFaceVisible = true;
                } else if (eastNeighbor.topType == TopType.Slope && (eastNeighbor.topDirection == Direction.W || eastNeighbor.topDirection == Direction.N)) {
                    isEastFaceVisible = false;
                } else if (eastNeighbor.topType == TopType.Corner && eastNeighbor.topDirection == Direction.NW) {
                    isEastFaceVisible = false;
                }

                if (isEastFaceVisible)
                {
                    AddTriangle(
                        center + CellMetrics.corners[2],
                        centerTop + CellMetrics.corners[1],
                        center + CellMetrics.corners[1],
                        cell.baseTexture,
                        RelativeDirection.LR
                    );
                }
            }
            else if (cell.topDirection == Direction.SE)
            {
                //West face

                AddTriangle(
                    center + CellMetrics.corners[0],
                    centerTop + CellMetrics.corners[2],
                    center + CellMetrics.corners[3],
                    cell.topTexture,
                    RelativeDirection.LR
                );

                //North face

                AddTriangle(
                    center + CellMetrics.corners[1],
                    centerTop + CellMetrics.corners[2],
                    center + CellMetrics.corners[0],
                    cell.topTexture,
                    RelativeDirection.LL
                );

                //East face

                if (eastNeighbor == null) {
                    isEastFaceVisible = true;
                } else if (eastNeighbor.Elevation < cell.Elevation) {
                    isEastFaceVisible = true;
                } else if (eastNeighbor.Elevation > cell.Elevation) {
                    isEastFaceVisible = false;
                }else if (eastNeighbor.topType != TopType.Slope && eastNeighbor.topType != TopType.Corner) {
                    isEastFaceVisible = true;
                } else if (eastNeighbor.topType == TopType.Slope && (eastNeighbor.topDirection == Direction.W || eastNeighbor.topDirection == Direction.S)) {
                    isEastFaceVisible = false;
                } else if (eastNeighbor.topType == TopType.Corner && eastNeighbor.topDirection == Direction.SW) {
                    isEastFaceVisible = false;
                }

                if (isEastFaceVisible)
                {
                    AddTriangle(
                        center + CellMetrics.corners[2],
                        centerTop + CellMetrics.corners[2],
                        center + CellMetrics.corners[1],
                        cell.baseTexture,
                        RelativeDirection.LL
                    );
                }

                // South face

                if (southNeighbor == null) {
                    isSouthFaceVisible = true;
                } else if (southNeighbor.Elevation < cell.Elevation) {
                    isSouthFaceVisible = true;
                } else if (southNeighbor.Elevation > cell.Elevation) {
                    isSouthFaceVisible = false;
                }else if (southNeighbor.topType != TopType.Slope && southNeighbor.topType != TopType.Corner) {
                    isSouthFaceVisible = true;
                } else if (southNeighbor.topType == TopType.Slope && (southNeighbor.topDirection == Direction.E || southNeighbor.topDirection == Direction.N)) {
                    isSouthFaceVisible = false;
                } else if (southNeighbor.topType == TopType.Corner && southNeighbor.topDirection == Direction.NE) {
                    isSouthFaceVisible = false;
                }

                if (isSouthFaceVisible)
                {
                    AddTriangle(
                        center + CellMetrics.corners[3],
                        centerTop + CellMetrics.corners[2],
                        center + CellMetrics.corners[2],
                        cell.baseTexture,
                        RelativeDirection.LR
                    );
                }
            }
            else if (cell.topDirection == Direction.SW)
            {
                //North face

                AddTriangle(
                    center + CellMetrics.corners[1],
                    centerTop + CellMetrics.corners[3],
                    center + CellMetrics.corners[0],
                    cell.topTexture,
                    RelativeDirection.LR
                );

                //East face

                AddTriangle(
                    center + CellMetrics.corners[2],
                    centerTop + CellMetrics.corners[3],
                    center + CellMetrics.corners[1],
                    cell.topTexture,
                    RelativeDirection.LL
                );

                // South face

                if (southNeighbor == null) {
                    isSouthFaceVisible = true;
                } else if (southNeighbor.Elevation < cell.Elevation) {
                    isSouthFaceVisible = true;
                } else if (southNeighbor.Elevation > cell.Elevation) {
                    isSouthFaceVisible = false;
                }else if (southNeighbor.topType != TopType.Slope && southNeighbor.topType != TopType.Corner) {
                    isSouthFaceVisible = true;
                } else if (southNeighbor.topType == TopType.Slope && (southNeighbor.topDirection == Direction.N || southNeighbor.topDirection == Direction.W)) {
                    isSouthFaceVisible = false;
                } else if (southNeighbor.topType == TopType.Corner && southNeighbor.topDirection == Direction.NW) {
                    isSouthFaceVisible = false;
                }

                if (isSouthFaceVisible)
                {
                    AddTriangle(
                        center + CellMetrics.corners[3],
                        centerTop + CellMetrics.corners[3],
                        center + CellMetrics.corners[2],
                        cell.baseTexture,
                        RelativeDirection.LL
                    );
                }

                //West face

                if (westNeighbor == null) {
                    isWestFaceVisible = true;
                } else if (westNeighbor.Elevation < cell.Elevation) {
                    isWestFaceVisible = true;
                } else if (westNeighbor.Elevation > cell.Elevation) {
                    isWestFaceVisible = false;
                }else if (westNeighbor.topType != TopType.Slope && westNeighbor.topType != TopType.Corner) {
                    isWestFaceVisible = true;
                } else if (westNeighbor.topType == TopType.Slope && (westNeighbor.topDirection == Direction.E || westNeighbor.topDirection == Direction.S)) {
                    isWestFaceVisible = false;
                } else if (westNeighbor.topType == TopType.Corner && westNeighbor.topDirection == Direction.SE) {
                    isWestFaceVisible = false;
                }

                if (isWestFaceVisible)
                {
                    AddTriangle(
                        center + CellMetrics.corners[0],
                        centerTop + CellMetrics.corners[3],
                        center + CellMetrics.corners[3],
                        cell.baseTexture,
                        RelativeDirection.LR
                    );
                }
            }
            else if (cell.topDirection == Direction.NW)
            {
                //East face

                AddTriangle(
                    center + CellMetrics.corners[2],
                    centerTop + CellMetrics.corners[0],
                    center + CellMetrics.corners[1],
                    cell.topTexture,
                    RelativeDirection.LR
                );

                //South face

                AddTriangle(
                    center + CellMetrics.corners[3],
                    centerTop + CellMetrics.corners[0],
                    center + CellMetrics.corners[2],
                    cell.topTexture,
                    RelativeDirection.LL
                );

                //West face

                if (westNeighbor == null) {
                    isWestFaceVisible = true;
                } else if (westNeighbor.Elevation < cell.Elevation) {
                    isWestFaceVisible = true;
                } else if (westNeighbor.Elevation > cell.Elevation) {
                    isWestFaceVisible = false;
                }else if (westNeighbor.topType != TopType.Slope && westNeighbor.topType != TopType.Corner) {
                    isWestFaceVisible = true;
                } else if (westNeighbor.topType == TopType.Slope && (westNeighbor.topDirection == Direction.W || westNeighbor.topDirection == Direction.N)) {
                    isWestFaceVisible = false;
                } else if (westNeighbor.topType == TopType.Corner && westNeighbor.topDirection == Direction.NE) {
                    isWestFaceVisible = false;
                }

                if (isWestFaceVisible)
                {
                    AddTriangle(
                        center + CellMetrics.corners[0],
                        centerTop + CellMetrics.corners[0],
                        center + CellMetrics.corners[3],
                        cell.baseTexture,
                        RelativeDirection.LL
                    );
                }

                // North face

                if (northNeighbor == null) {
                    isNorthFaceVisible = true;
                } else if (northNeighbor.Elevation < cell.Elevation) {
                    isNorthFaceVisible = true;
                } else if (northNeighbor.Elevation > cell.Elevation) {
                    isNorthFaceVisible = false;
                }else if (northNeighbor.topType != TopType.Slope && northNeighbor.topType != TopType.Corner) {
                    isNorthFaceVisible = true;
                } else if (northNeighbor.topType == TopType.Slope && (northNeighbor.topDirection == Direction.S || northNeighbor.topDirection == Direction.W)) {
                    isNorthFaceVisible = false;
                } else if (northNeighbor.topType == TopType.Corner && northNeighbor.topDirection == Direction.SW) {
                    isNorthFaceVisible = false;
                }

                if (isNorthFaceVisible)
                {
                    AddTriangle(
                        center + CellMetrics.corners[1],
                        centerTop + CellMetrics.corners[0],
                        center + CellMetrics.corners[0],
                        cell.baseTexture,
                        RelativeDirection.LR
                    );
                }
            }
        }

        //Sides

        if (southNeighbor != null)
        {
            //South faces, excluding map edges
            if (southNeighbor.Elevation < cell.Elevation)
            {
                AddQuad(
                    southNeighbor.transform.localPosition + CellMetrics.corners[0],
                    cell.transform.localPosition + CellMetrics.corners[3],
                    cell.transform.localPosition + CellMetrics.corners[2],
                    southNeighbor.transform.localPosition + CellMetrics.corners[1],
                    cell.baseTexture
                );
            }
        }
        else //southNeighbor == null
        {
            //Southern edge faces
            if (cell.Elevation > 0)
            {
                Vector3 baseCenter = center - (new Vector3(0, cell.Elevation * CellMetrics.offset, 0));
                AddQuad(
                    baseCenter + CellMetrics.corners[3],
                    cell.transform.localPosition + CellMetrics.corners[3],
                    cell.transform.localPosition + CellMetrics.corners[2],
                    baseCenter + CellMetrics.corners[2],
                    cell.baseTexture
                );
            }
        }
        
        if (northNeighbor != null)
        {
            //North faces, excluding map edges
            if (northNeighbor.Elevation < cell.Elevation)
            {
                AddQuad(
                    northNeighbor.transform.localPosition + CellMetrics.corners[2],
                    cell.transform.localPosition + CellMetrics.corners[1],
                    cell.transform.localPosition + CellMetrics.corners[0],
                    northNeighbor.transform.localPosition + CellMetrics.corners[3],
                    cell.baseTexture
                );
            }
        }
        // North edge faces
        else //if (northNeighbor == null)
        {
            if (cell.Elevation > 0)
            {
                Vector3 baseCenter = center - (new Vector3(0, cell.Elevation * CellMetrics.offset, 0));
                AddQuad(
                    baseCenter + CellMetrics.corners[1],
                    cell.transform.localPosition + CellMetrics.corners[1],
                    cell.transform.localPosition + CellMetrics.corners[0],
                    baseCenter + CellMetrics.corners[0],
                    cell.baseTexture
                );
            }
        }

        if (eastNeighbor != null)
        {
            //East faces, excluding map edges
            if (eastNeighbor.Elevation < cell.Elevation)
            {
                AddQuad(
                    eastNeighbor.transform.localPosition + CellMetrics.corners[3],
                    cell.transform.localPosition + CellMetrics.corners[2],
                    cell.transform.localPosition + CellMetrics.corners[1],
                    eastNeighbor.transform.localPosition + CellMetrics.corners[0],
                    cell.baseTexture
                );
            }
        }
        else //eastNeighbor == null
        {
            //East edge faces
            if (cell.Elevation > 0)
            {
                Vector3 baseCenter = center - (new Vector3(0, cell.Elevation * CellMetrics.offset, 0));
                AddQuad(
                    baseCenter + CellMetrics.corners[2],
                    cell.transform.localPosition + CellMetrics.corners[2],
                    cell.transform.localPosition + CellMetrics.corners[1],
                    baseCenter + CellMetrics.corners[1],
                    cell.baseTexture
                );
            }
        }

        if (westNeighbor != null)
        {
            //West faces, excluding map edges
            if (westNeighbor.Elevation < cell.Elevation)
            {
                AddQuad(
                    westNeighbor.transform.localPosition + CellMetrics.corners[1],
                    cell.transform.localPosition + CellMetrics.corners[0],
                    cell.transform.localPosition + CellMetrics.corners[3],
                    westNeighbor.transform.localPosition + CellMetrics.corners[2],
                    cell.baseTexture
                );
            }
        }
        // West edge faces
        else //if (westNeighbor == null)
        {
            if (cell.Elevation > 0)
            {
                Vector3 baseCenter = center - (new Vector3(0, cell.Elevation * CellMetrics.offset, 0));
                AddQuad(
                    baseCenter + CellMetrics.corners[3],
                    cell.transform.localPosition + CellMetrics.corners[3],
                    cell.transform.localPosition + CellMetrics.corners[0],
                    baseCenter + CellMetrics.corners[0],
                    cell.baseTexture
                );
            }
        }
    } 

    //BottomLeft, TopLeft, TopRight, BottomRight
    void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, AtlasTexture atlasTexture)
    {
        int vertexIndex = vertices.Count;

        vertices.Add(v0);
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        //v1,v2,v3
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);

        //v1,v3,v4
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);

        AddQuadTexture(atlasTexture);
    }

    void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2, AtlasTexture atlasTexture, RelativeDirection rightAngle)
    {
        int vertexIndex = vertices.Count;

        vertices.Add(v0);
        vertices.Add(v1);
        vertices.Add(v2);

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);

        AddTriangleTexture(atlasTexture, rightAngle);
    }

    Vector2 GetCoordinatesOfAtlasTexture(AtlasTexture atlasTexture)
    {
        int atlasTextureIndex = (int)atlasTexture;

        if (atlasTextureIndex == 0)
        {
            return Vector2.zero;
        }
        else
        {
            int x = atlasTextureIndex / 10;
            int y = atlasTextureIndex % 10;
            return new Vector2(x, y);
        }
    }
    
    /// <summary> Returns UVData(uMin, uMax, vMin, vMax) </summary>
    /// <param name="atlasTexture"></param>
    /// <returns></returns>
    public UVData GetUVDataFor(AtlasTexture atlasTexture)
    {
        Vector2 textureCoordinates = GetCoordinatesOfAtlasTexture(atlasTexture);

        float uMin = portionOfAtlas * textureCoordinates.x;// + textureReadOffset;
        float uMax = portionOfAtlas * (textureCoordinates.x + 1);// - textureReadOffset;
        float vMin = portionOfAtlas * textureCoordinates.y;// + textureReadOffset;
        float vMax = portionOfAtlas * (textureCoordinates.y + 1);// - textureReadOffset;

        return new UVData(uMin, uMax, vMin, vMax);
    }

    void AddTriangleTexture(AtlasTexture atlasTexture, RelativeDirection rightAngle)
    {
        UVData uvData = GetUVDataFor(atlasTexture);

        if (rightAngle == RelativeDirection.LL)
        {
            uvs.Add(new Vector2(uvData.uMin, uvData.vMin));
            uvs.Add(new Vector2(uvData.uMin, uvData.vMax));
            uvs.Add(new Vector2(uvData.uMax, uvData.vMin));
        }
        else if (rightAngle == RelativeDirection.LR)
        {
            uvs.Add(new Vector2(uvData.uMin, uvData.vMin));
            uvs.Add(new Vector2(uvData.uMax, uvData.vMax));
            uvs.Add(new Vector2(uvData.uMax, uvData.vMin));
        }
    }

    void AddQuadTexture(AtlasTexture atlasTexture)
    {
        UVData uvData = GetUVDataFor(atlasTexture);

        uvs.Add(new Vector2(uvData.uMin, uvData.vMin));
        uvs.Add(new Vector2(uvData.uMin, uvData.vMax));
        uvs.Add(new Vector2(uvData.uMax, uvData.vMax));
        uvs.Add(new Vector2(uvData.uMax, uvData.vMin));
    }
}
public struct UVData
{
    public float uMin, uMax, vMin, vMax;
    
    public UVData(float uMin, float uMax, float vMin, float vMax)
    {
        this.uMin = uMin;
        this.uMax = uMax;
        this.vMin = vMin;
        this.vMax = vMax;
    }
}
