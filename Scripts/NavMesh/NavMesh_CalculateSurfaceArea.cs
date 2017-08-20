using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMesh_CalculateSurfaceArea : MonoBehaviour {

	// DECLARATIONS

	// NavMesh
	Vector3[] vertices;
	int[] triangles; 

	Mesh mesh;

	Vector3 specificLoc;

	Vector3[] randomPositions;

	void Start () {

		Initialize ();

		//float calculate = CalculateSurfaceAreas ();

		#region Test Triangle Area Calculation

		// Vector2

//		Vector2[] pts = new Vector2[3];
//		pts [0] = new Vector2 (2, 5);
//		pts [1] = new Vector2 (5, 0);
//		pts [2] = new Vector2 (0, 0);
//
// 		// Distance between two points = sqrt(  (x2 - x1)^2 + (y2 - y1)^2 )
//		float[] dist = new float[3];
//		dist [0] = Mathf.Sqrt ( Mathf.Pow(pts[1].x - pts[0].x, 2) + Mathf.Pow(pts[1].y - pts[0].y, 2) );
//		dist [1] = Mathf.Sqrt ( Mathf.Pow(pts[2].x - pts[1].x, 2) + Mathf.Pow(pts[2].y - pts[1].y, 2) );
//		dist [2] = Mathf.Sqrt ( Mathf.Pow(pts[0].x - pts[2].x, 2) + Mathf.Pow(pts[0].y - pts[2].y, 2) );
//
// 		// Area of a triangle = sqrt( s * (s - a) * (s - b) * (s - c) ) where a, b, c are sides
//		float s = ( dist[0] + dist[1] + dist[2] ) / 2;
//		float area = Mathf.Sqrt ( s * (s - dist[0]) * (s - dist[1]) * (s - dist[2]) );
//
//		Debug.Log("V2 - CALCULATE TRIANGLE AREA~");
//		Debug.Log ("Points: 1 - " + pts[0] + ", 2 - " + pts[1] + ", 3 - " + pts[2]);
//		Debug.Log ("Distances: 1 - " + dist[0] + ", 2 - " + dist[1] + ", 3 - " + dist[2]);
//		Debug.Log ("Semi-Perimeter: " + s);
//		Debug.Log ("Area: " + area);

		// --------------

		// Vector3

//		Vector3[] pts = new Vector3[3];
//		pts[0] = new Vector3(3, 4, 5);
//		pts[1] = new Vector3(6, 2, 9);
//		pts[2] = new Vector3(0, 1, 7);
//
//		// Area of a triangle = ( b - a ) X ( c - a) <= Cross Product
//		Vector3 side1 = pts[1] - pts[0];
//		Vector3 side2 = pts[2] - pts[0];
//		float area = Vector3.Cross(side1, side2).magnitude / 2;
//
//		Debug.Log("V3 - CALCULATE TRIANGLE AREA~");
//		Debug.Log ("Points: 1 - " + pts[0] + ", 2 - " + pts[1] + ", 3 - " + pts[2]);
//		Debug.Log ("Sides: 1 - " + side1 + ", 2 - " + side2);
//		Debug.Log ("Area: " + area);
		#endregion
	}

	void Initialize(){

		mesh = new Mesh ();

		// assign mesh information
		vertices = NavMesh.CalculateTriangulation ().vertices;
		triangles = NavMesh.CalculateTriangulation ().indices;

		// assign mesh info to the stored mesh
		mesh.vertices = vertices;
		mesh.triangles = triangles;

		randomPositions = new Vector3[26];
		for (int i = 0; i < randomPositions.Length; i++) {
			randomPositions[i] = GetRandomPositionOnMesh ( mesh);
		}

	}

	float[] CalculateSurfaceAreas (){

		// Triangles stores all points for every triangle~ 
		//		That means that there are 3 points per triangle
		int totalTriangles = triangles.Length / 3;

		// We're going to store our calculated areas in here and return this
		float[] surfaceAreas = new float[totalTriangles];

		for (int i = 0; i < totalTriangles; i++) {

			Vector3[] points = new Vector3[3];
			points [0] = vertices [triangles [i * 3 + 0]];	// if i = 0 we get point 0, 1, and 2~
			points [1] = vertices [triangles [i * 3 + 1]];
			points [2] = vertices [triangles [i * 3 + 2]];

			// Distance between two points = sqrt(  (x2 - x1)^2 + (y2 - y1)^2 )
			float[] dist = new float[3];
			dist [0] = (points[0] - points[1]).magnitude;
			dist [1] = (points[0] - points[1]).magnitude;
			dist [2] = (points[0] - points[1]).magnitude;

			// Area of a triangle = sqrt( s * (s - a) * (s - b) * (s - c) ) where a, b, c are sides
			float s = ( dist[0] + dist[1] + dist[2] ) / 2;
			surfaceAreas[i] = Mathf.Sqrt ( s * (s - dist[0]) * (s - dist[1]) * (s - dist[2]) );

		}

		return surfaceAreas;
	}

	public static Vector3 GetRandomPositionOnMesh(Mesh mesh){

		// Get a barycentric vector3 coordinate randomly
		Vector3 barycentric = new Vector3 (Random.value, Random.value, Random.value);

		while (barycentric == Vector3.zero) barycentric = new Vector3 (Random.value, Random.value, Random.value);

		barycentric /= (barycentric.x + barycentric.y + barycentric.z);

		// Convert to a local position
		int randomTriangle = Random.Range(0, mesh.triangles.Length/3);

		Vector3[] pts = new Vector3[3];
		pts [0] = mesh.vertices [mesh.triangles [randomTriangle * 3 + 0]];
		pts [1] = mesh.vertices [mesh.triangles [randomTriangle * 3 + 1]];
		pts [2] = mesh.vertices [mesh.triangles [randomTriangle * 3 + 2]];

		Vector3 position = ((pts [0] * barycentric.x) + (pts [1] * barycentric.y) + (pts [2] * barycentric.z));

		return position;
	}

	public static Vector3 GetRandomPositionOnNavMesh(){

		// Get a barycentric vector3 coordinate randomly
		Vector3 barycentric = new Vector3 (Random.value, Random.value, Random.value);

		while (barycentric == Vector3.zero) barycentric = new Vector3 (Random.value, Random.value, Random.value);

		barycentric /= (barycentric.x + barycentric.y + barycentric.z);

		// Find the NavMesh calculations
		Vector3[] vertices = NavMesh.CalculateTriangulation ().vertices;
		int[] triangles = NavMesh.CalculateTriangulation ().indices; 

		// Convert to a local position
		int randomTriangle = Random.Range(0, triangles.Length/3);

		Vector3[] pts = new Vector3[3];
		pts [0] = vertices [triangles [randomTriangle * 3 + 0]];
		pts [1] = vertices [triangles [randomTriangle * 3 + 1]];
		pts [2] = vertices [triangles [randomTriangle * 3 + 2]];

		Vector3 position = ((pts [0] * barycentric.x) + (pts [1] * barycentric.y) + (pts [2] * barycentric.z));

		return position;
	}

	void Update () {

	}
}
