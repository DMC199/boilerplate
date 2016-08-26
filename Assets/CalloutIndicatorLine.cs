using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CalloutIndicatorLine : MonoBehaviour {



	// Use this for initialization
	void Start () {
        MeshFilter filter = GetComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        Vector3[] Vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(1, 0, 1), new Vector3(1, 0, -1), new Vector3(-1, 0, -1) };
        Vector2[] UV = new Vector2[] { new Vector2(0, 256), new Vector2(256, 256), new Vector2(256, 0), new Vector2(0, 0) };

        int[] Triangles = new int[] { 0, 1, 2, 0, 2, 3 };

        mesh.vertices = Vertices;
        mesh.uv = UV;
        mesh.triangles = Triangles;

        filter.mesh = mesh;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
