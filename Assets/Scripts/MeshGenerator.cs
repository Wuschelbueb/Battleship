﻿#pragma warning disable 0649 
// Disable warnings that stuff is not beeing assigned 
// even though they are assigned throu the editor
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshGenerator {

    public static GameObject CreateCompass (float Radius = 5f, float Thickness = 0.5f, float ArrowSize = 2f, int NCirclePoints = 40) {
        
        GameObject go = new GameObject("Compass");
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();

        List<Vector3> outerCirclePoints = 
            Enumerable.Range(0, NCirclePoints + 1)
                      .Select(i => new Vector3(Mathf.Sin(2 * Mathf.PI * i / NCirclePoints), 0f, Mathf.Cos(2 * Mathf.PI * i / NCirclePoints))) 
                      .Select(v => (.5f * Thickness + Radius) * v)
                      .ToList();
        
        List<Vector3> innerCirclePoints =
            Enumerable.Range(0, NCirclePoints + 1)
                      .Select(i => new Vector3(Mathf.Sin(2 * Mathf.PI * i / NCirclePoints), 0f, Mathf.Cos(2 * Mathf.PI * i / NCirclePoints)))
                      .Select(v => (-0.5f * Thickness + Radius) * v)
                      .ToList();

        MeshGenerator mg = new MeshGenerator();

        for (int i = 0; i < NCirclePoints; i++)
        {
            mg.AddRectangle(outerCirclePoints[i+1], outerCirclePoints[i], innerCirclePoints[i], innerCirclePoints[i+1]);
            mg.AddRectangle(outerCirclePoints[i], outerCirclePoints[i+1], innerCirclePoints[i+1], innerCirclePoints[i]);
        }

        float[] Sizes = new float[] { 3f/2, 1f/3, 1f/2, 1f/3, 1f/2, 1f/3, 1f/2, 1f/3 };

        for (int i = 0; i < Sizes.Length; i++)
        {

            float angle = 2 * Mathf.PI * i / Sizes.Length;
            float something = Mathf.PI * 2 / 40;

            Vector3 center = Radius * new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
            Vector3 tip = ((Radius + Sizes[i] * ArrowSize) / Radius) * center;

            Vector3 left = Radius * new Vector3(Mathf.Sin(angle + something), 0f, Mathf.Cos(angle + something));
            Vector3 right = Radius * new Vector3(Mathf.Sin(angle - something), 0f, Mathf.Cos(angle - something));

            mg.AddTriangle(left, tip, right);
            mg.AddTriangle(left, right, tip);

        }



        mf.mesh = mg.GetMesh();

        return go;
    }


    public static void MeshGenTest () {
        GameObject go = new GameObject("Test");
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();

        MeshGenerator mg = new MeshGenerator();
        mg.AddRectangle(Vector3.zero, Vector3.right, Vector3.one, Vector3.forward);

        mf.mesh = mg.GetMesh();

    }

    List<Vector3> Vertices;
    List<int> Triangles;

    public MeshGenerator () {
        Vertices = new List<Vector3>();
        Triangles = new List<int>();
    }

    public void AddTriangle (Vector3 a, Vector3 b, Vector3 c) {
        Triangles.Add(Vertices.Count);
        Vertices.Add(a);
        Triangles.Add(Vertices.Count);
        Vertices.Add(c);
        Triangles.Add(Vertices.Count);
        Vertices.Add(b);
    }

	public void AddRectangle (params Vector3[] vecs) {
		Debug.Assert (vecs.Length == 4);
		AddRectangle (vecs [0], vecs [1], vecs [2], vecs [3]);
	}

    public void AddRectangle (Vector3 a, Vector3 b, Vector3 c, Vector3 d){
		// Fed up with mixing up sides, just draw all of them.
        AddTriangle(a,b,d);
        AddTriangle(b,c,d);

		AddTriangle(a,d,b);
		AddTriangle(b,d,c);
    }

    public Mesh GetMesh () {
        Mesh mesh = new Mesh();
        mesh.vertices = Vertices.ToArray();
        mesh.triangles = Triangles.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    public static MeshData GenerateTerrainMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail)
    {
       
        int skipIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int numVertsPerLine = meshSettings.numVertsPerLine;

        Vector2 topLeft = new Vector2(-1, 1) * meshSettings.meshWorldSize / 2f;

        MeshData meshData = new MeshData(numVertsPerLine, skipIncrement, meshSettings.useFlatShading);

        int[,] vertexIndicesMap = new int[numVertsPerLine, numVertsPerLine];
        int meshVertexIndex = 0;
        int outOfMeshVertexIndex = -1;

        for (int y = 0; y < numVertsPerLine; y++)
        {
            for (int x = 0; x < numVertsPerLine; x++)
            {
                bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;
                bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);
                if (isOutOfMeshVertex)
                {
                    vertexIndicesMap[x, y] = outOfMeshVertexIndex;
                    outOfMeshVertexIndex--;
                }
                else if (!isSkippedVertex)
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for (int y = 0; y < numVertsPerLine; y++)
        {
            for (int x = 0; x < numVertsPerLine; x++)
            {
                bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);

                if (!isSkippedVertex)
                {
                    bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;
                    bool isMeshEdgeVertex = (y == 1 || y == numVertsPerLine - 2 || x == 1 || x == numVertsPerLine - 2) && !isOutOfMeshVertex;
                    bool isMainVertex = (x - 2) % skipIncrement == 0 && (y - 2) % skipIncrement == 0 && !isOutOfMeshVertex && !isMeshEdgeVertex;
                    bool isEdgeConnectionVertex = (y == 2 || y == numVertsPerLine - 3 || x == 2 || x == numVertsPerLine - 3) && !isOutOfMeshVertex && !isMeshEdgeVertex && !isMainVertex;

                    int vertexIndex = vertexIndicesMap[x, y];
                    Vector2 percent = new Vector2(x - 1, y - 1) / (numVertsPerLine - 3);
                    Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, -percent.y) * meshSettings.meshWorldSize;
                    float height = heightMap[x, y];

                    if (isEdgeConnectionVertex)
                    {
                        bool isVertical = x == 2 || x == numVertsPerLine - 3;
                        int dstToMainVertexA = ((isVertical) ? y - 2 : x - 2) % skipIncrement;
                        int dstToMainVertexB = skipIncrement - dstToMainVertexA;
                        float dstPercentFromAToB = dstToMainVertexA / (float)skipIncrement;

                        float heightMainVertexA = heightMap[(isVertical) ? x : x - dstToMainVertexA, (isVertical) ? y - dstToMainVertexA : y];
                        float heightMainVertexB = heightMap[(isVertical) ? x : x + dstToMainVertexB, (isVertical) ? y + dstToMainVertexB : y];

                        height = heightMainVertexA * (1 - dstPercentFromAToB) + heightMainVertexB * dstPercentFromAToB;
                    }

                    meshData.AddVertex(new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), percent, vertexIndex);

                    bool createTriangle = x < numVertsPerLine - 1 && y < numVertsPerLine - 1 && (!isEdgeConnectionVertex || (x != 2 && y != 2));

                    if (createTriangle)
                    {
                        int currentIncrement = (isMainVertex && x != numVertsPerLine - 3 && y != numVertsPerLine - 3) ? skipIncrement : 1;

                        int a = vertexIndicesMap[x, y];
                        int b = vertexIndicesMap[x + currentIncrement, y];
                        int c = vertexIndicesMap[x, y + currentIncrement];
                        int d = vertexIndicesMap[x + currentIncrement, y + currentIncrement];
                        meshData.AddTriangle(a, d, c);
                        meshData.AddTriangle(d, a, b);
                    }
                }
            }
        }

        meshData.ProcessMesh();

        return meshData;

    }
}

public class MeshData
{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    Vector3[] bakedNormals;

    Vector3[] outOfMeshVertices;
    int[] outOfMeshTriangles;

    int triangleIndex;
    int outOfMeshTriangleIndex;

    bool useFlatShading;

    public MeshData(int numVertsPerLine, int skipIncrement, bool useFlatShading)
    {
        this.useFlatShading = useFlatShading;

        int numMeshEdgeVertices = (numVertsPerLine - 2) * 4 - 4;
        int numEdgeConnectionVertices = (skipIncrement - 1) * (numVertsPerLine - 5) / skipIncrement * 4;
        int numMainVerticesPerLine = (numVertsPerLine - 5) / skipIncrement + 1;
        int numMainVertices = numMainVerticesPerLine * numMainVerticesPerLine;

        vertices = new Vector3[numMeshEdgeVertices + numEdgeConnectionVertices + numMainVertices];
        uvs = new Vector2[vertices.Length];

        int numMeshEdgeTriangles = 8 * (numVertsPerLine - 4);
        int numMainTriangles = (numMainVerticesPerLine - 1) * (numMainVerticesPerLine - 1) * 2;
        triangles = new int[(numMeshEdgeTriangles + numMainTriangles) * 3];

        outOfMeshVertices = new Vector3[numVertsPerLine * 4 - 4];
        outOfMeshTriangles = new int[24 * (numVertsPerLine - 2)];
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        if (vertexIndex < 0)
        {
            outOfMeshVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            outOfMeshTriangles[outOfMeshTriangleIndex] = a;
            outOfMeshTriangles[outOfMeshTriangleIndex + 1] = b;
            outOfMeshTriangles[outOfMeshTriangleIndex + 2] = c;
            outOfMeshTriangleIndex += 3;
        }
        else
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }
    }

    Vector3[] CalculateNormals()
    {

        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = outOfMeshTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = outOfMeshTriangles[normalTriangleIndex];
            int vertexIndexB = outOfMeshTriangles[normalTriangleIndex + 1];
            int vertexIndexC = outOfMeshTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0)
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }


        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;

    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? outOfMeshVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? outOfMeshVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? outOfMeshVertices[-indexC - 1] : vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public void ProcessMesh()
    {
        if (useFlatShading)
        {
            FlatShading();
        }
        else
        {
            BakeNormals();
        }
    }

    void BakeNormals()
    {
        bakedNormals = CalculateNormals();
    }

    void FlatShading()
    {
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadedVertices;
        uvs = flatShadedUvs;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        if (useFlatShading)
        {
            mesh.RecalculateNormals();
        }
        else
        {
            mesh.normals = bakedNormals;
        }
        return mesh;
    }

}