using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakMeshObject : MonoBehaviour {

	void Start () {
		
	}

	void Update () {

		if (Input.GetKeyDown (KeyCode.P)) {
			
			gameObject.AddComponent<TriangleExplosion> ();
			StartCoroutine (gameObject.GetComponent<TriangleExplosion> ().SplitMesh (true));
		}

	}
}
