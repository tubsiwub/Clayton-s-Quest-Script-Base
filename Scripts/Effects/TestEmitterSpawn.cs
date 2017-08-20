using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEmitterSpawn : MonoBehaviour {

	public ParticleSystem pSys;

	void Start () {
		
	}

	void Update () {

	}

	void OnTriggerEnter(Collider col){

		StartCoroutine (DeleteMe ());

	}

	IEnumerator DeleteMe(){

		pSys.Play ();

		this.GetComponent<BoxCollider> ().enabled = false;
		this.GetComponent<MeshRenderer> ().enabled = false;

		yield return new WaitForSeconds (2.0f);

		Destroy (this.transform.gameObject);

	}
}
