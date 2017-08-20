// TRISTAN

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Script used in NavMesh test scene and is not used for final game

public class NavMeshAgent_MoveTowardTarget : MonoBehaviour {

	public Transform target;

	NavMeshAgent agent;

	// NavMesh data
	Vector3[] vertices;
	int[] triangles;
		//int[] areas;

	Mesh mesh;
		//MeshFilter meshFilter;

	// temp
	Vector3 specificLoc;

	void Start () {
		agent = GetComponent<NavMeshAgent> ();
		mesh = new Mesh ();
			//meshFilter = new MeshFilter ();

		// assign mesh information
			//areas = NavMesh.CalculateTriangulation ().areas;
		vertices = NavMesh.CalculateTriangulation ().vertices;
		triangles = NavMesh.CalculateTriangulation ().indices;

		// assign mesh info to the stored mesh
		mesh.vertices = vertices;
		mesh.triangles = triangles;
			//meshFilter.mesh = mesh;

		specificLoc = transform.TransformPoint (vertices [4]);

		Debug.Log (specificLoc);
	}

	void Update () {

		agent.SetDestination (target.position);

		if (Vector3.Distance (transform.position, target.position) < 2) {
			GenerateNewLocation ();
		}
	}

	void GenerateNewLocation(){
		
		int randomIndex = Random.Range (0, vertices.Length);

		specificLoc = vertices [randomIndex];
		//specificLoc = transform.TransformPoint (vertices [randomIndex]);
		target.position = specificLoc;
	}
}
