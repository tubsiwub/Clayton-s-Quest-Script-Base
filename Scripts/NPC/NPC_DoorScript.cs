using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_DoorScript : MonoBehaviour {


	float radius = 3.5f;

	Animator anim;


	void Start () {

		anim = GetComponent<Animator> ();
	}


	void Update () {

		// Set collider radius here so it can be updated
		GetComponent<SphereCollider> ().radius = radius;


	}

	void OnTriggerEnter(Collider col){

		if (col.transform.tag == "Player" || col.transform.tag == "NPC") {
			anim.StopPlayback ();
			anim.SetBool ("Opened", true);
		}

	}

	void OnTriggerExit(Collider col){

		if (col.transform.tag == "Player" || col.transform.tag == "NPC") {
			anim.SetBool ("Opened", false);
		} 

	}
}
