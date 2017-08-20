using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpZone : MonoBehaviour {

	public Vector3 direction;

	public float gapLength = 3;

	// VALUES SET FOR A 3-wide GAP

	//[Range(1,10)] public 
	float powerX = 4;

	//[Range(1,10)] public 
	float powerY = 6;

	//[Range(1,5)] public 
	float timeout = 1.4f;

	void Start () 
	{
		powerX = gapLength + (gapLength * 0.33f);
		powerY = 6;
		timeout = 1.0f + (gapLength / 20);
	}

	void Update () 
	{
		
	}

	void OnTriggerEnter(Collider col)
	{

		if (col.tag == "Enemy") {

			GetComponent<BoxCollider> ().enabled = false;

			// If script is meant to be used then use it
			if(col.GetComponent<NavMeshAgentScript>().enabled)
				col.GetComponent<NavMeshAgentScript> ().JUMP (direction, powerX, powerY, timeout);

			StartCoroutine (ResetCollision (1.0f));

		}

	}

	IEnumerator ResetCollision(float time){

		yield return new WaitForSeconds(time);

		GetComponent<BoxCollider> ().enabled = true;

	}
}
