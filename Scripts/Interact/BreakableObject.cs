using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : MonoBehaviour {

	List<GameObject> pieceList;

	public bool CONVEX = false;

	void Start () {

		pieceList = new List<GameObject> ();

		// Fill piece list with pieces of breakable object
		for (int i = 0; i < this.transform.childCount; i++) {

			pieceList.Add (transform.GetChild(i).gameObject);

		}

		// Rename pieces in case they are named poorly
		foreach (GameObject obj in pieceList) {

			obj.name = "Piece";

		}

	}


	void Update () {



	}

	// For now, we break on trigger
	void OnTriggerEnter(){

		GetComponent<Collider> ().enabled = false;

		// Give all pieces the ability to fall all over the place
		foreach (GameObject obj in pieceList) {

			obj.AddComponent<Rigidbody> ();
			obj.AddComponent<MeshCollider> ();

			obj.GetComponent<MeshCollider> ().sharedMesh = obj.GetComponent<MeshFilter> ().mesh;

			if (CONVEX) {
				obj.GetComponent<MeshCollider> ().inflateMesh = true;
				obj.GetComponent<MeshCollider> ().convex = true;
			}

		}

	}

}
