using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hourglass_Spin : MonoBehaviour {

	ActivatedTimer timerRef;

	bool timerEnabled = false;

	void Start(){

		timerRef = transform.parent.GetComponent<ActivatedTimer> ();

		// Events
		timerRef.OnTimerStart += StartTimer;
		timerRef.OnTimerRunOut += EndTimer;

		// Init
		GetComponent<Image> ().enabled = false;

	}

	void StartTimer(){
		
		timerEnabled = true;

		GetComponent<Animator> ().SetBool ("Timer", timerEnabled);

		GetComponent<Image> ().enabled = timerEnabled;

	}

	void EndTimer(){

		timerEnabled = false;

		GetComponent<Animator> ().SetBool ("Timer", timerEnabled);

		GetComponent<Image> ().enabled = timerEnabled;

	}
}
