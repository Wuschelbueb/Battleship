using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class BorderCreator : MonoBehaviour {

	[SerializeField] List<Transform> CornerPoints;
	[SerializeField] float size;
	[SerializeField] Material material;

	void Awake () {
		MeshFilter mf = gameObject.AddComponent<MeshFilter> ();
		MeshRenderer mr = gameObject.AddComponent<MeshRenderer> ();

		float halfsize = size / 2;

		MeshGenerator mg = new MeshGenerator();

		//mg.AddRectangle (CornerPoints [1].position, CornerPoints [2].position, CornerPoints [3].position, CornerPoints [0].position);
		// TODO calc the actual borders;

		mf.mesh = mg.GetMesh ();
		mr.material = material;
	}


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnDrawGizmos() {
		CornerPoints.Add (CornerPoints.First());
		if (CornerPoints == null) return;    // <- as this is also executed in edit mode. 
		for (int i = 0; i < CornerPoints.Count - 1; i++) {
			Gizmos.DrawLine (CornerPoints [i].position, CornerPoints [i + 1].position);
		}
		CornerPoints.Remove (CornerPoints.Last());
	}

}
