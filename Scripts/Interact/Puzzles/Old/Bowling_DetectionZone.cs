using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bowling_DetectionZone : MonoBehaviour {

	// Events
	public delegate void BowlingZone_Success();
	public event BowlingZone_Success OnZoneEnter;

	GameObject playerObj;

	public GameObject bowlingStartZone;
	public GameObject bowlingEvent;

	void Start(){

		playerObj = GameObject.FindWithTag ("Player");

	}

	void OnTriggerEnter(Collider col){

		if (col.transform.tag == "Player") {
			if (playerObj.GetComponent<BallController> ().GetSpeed > 0.8f) {

				if (OnZoneEnter != null)
					OnZoneEnter ();

			}
		}
	}
}
