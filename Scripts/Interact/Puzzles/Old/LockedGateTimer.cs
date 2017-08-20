using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// USE ActivationGate.cs INSTEAD !!!

// WARNING:  DEPRECIATED 



public class LockedGateTimer : MonoBehaviour {

//
//
//	// Make sure TimerObj is named correctly and within the same parent as the GateObj
//	GameObject timerObj;
//
//
//	public Vector3 openLocation;
//	Vector3 closedLocation;
//
//
//	Puzzle_WinZone winZone;
//
//	bool winState = false;
//
//
//	void Start () {
//
//		foreach (Transform child in transform.parent)
//			if (child.name == "TimerObj")
//				timerObj = child.gameObject;
//
//		// References
//		winZone = GameObject.Find(transform.parent.name + "/WinZone").GetComponent<Puzzle_WinZone>();
//		winZone.OnWinZoneActivate += Complete;
//
//		// Set the function as an event
//		timerObj.GetComponent<ActivatedTimer> ().OnTimerRunOut += GateReset;
//		timerObj.GetComponent<ActivatedTimer> ().OnTimerStart += GateMove;
//
//		closedLocation = transform.localPosition;
//
//		// Start the gate as closed
//		transform.localPosition = closedLocation;
//
//	}
//
//
//	void Complete(){
//
//		winState = true;
//
//		StartCoroutine (ShiftGateOpen ());
//
//	}
//
//
//	void GateReset() {
//
//		if(!winState)
//			StartCoroutine (ShiftGateClosed ());
//
//	}
//
//	void GateMove(){
//
//		StartCoroutine (ShiftGateOpen ());
//
//	}
//
//	IEnumerator ShiftGateOpen(){
//
//		float counter = 2;
//
//		while (counter > 0) {
//		
//			transform.localPosition = Vector3.Lerp (transform.localPosition, openLocation, Time.deltaTime);
//
//			counter -= Time.deltaTime;
//
//			yield return new WaitForEndOfFrame ();
//
//		}
//
//	}
//
//	IEnumerator ShiftGateClosed(){
//
//		float counter = 2;
//
//		while (counter > 0) {
//
//			transform.localPosition = Vector3.Lerp (transform.localPosition, closedLocation, 4 * Time.deltaTime);
//
//			counter -= Time.deltaTime;
//
//			yield return new WaitForEndOfFrame ();
//
//		}
//
//	}
}
