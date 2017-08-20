using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlingStartZone : MonoBehaviour {

	// Events
	public delegate void Bowling_EnterZone();
	public event Bowling_EnterZone OnZoneEnter;

	public delegate void Bowling_GameEnd();
	public event Bowling_GameEnd OnGameEnd;

	GameObject bowlingEventObj;

	public BoxCollider triggerBox;

	void Start () {

		foreach (Transform child in transform.parent) {
			
			if (child.name == "BowlingEvent")
				bowlingEventObj = child.gameObject;
		
		}

		if (bowlingEventObj != null) {

			// An event firing an event
			bowlingEventObj.GetComponent<BowlingEvent> ().OnBowlingSuccess += GameEnd;

		}

	}

	void GameEnd(){

		if (OnGameEnd != null)
			OnGameEnd ();

		triggerBox.enabled = false;

	}

	void OnTriggerEnter(Collider col){

		if (col.transform.tag == "Player") {

			if (OnZoneEnter != null)
				OnZoneEnter ();

		}

	}

	void OnCollisionEnter(Collision col){

		if (col.transform.tag == "Player") {

			if(col.transform.GetComponent<PlayerHandler> ().IsFrozen)
				col.transform.GetComponent<PlayerHandler> ().SetFrozen (false, true);

		}

	}

}
