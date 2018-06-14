#pragma warning disable 0649 
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

}

