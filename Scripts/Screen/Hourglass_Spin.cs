using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hourglass_Spin : MonoBehaviour {

	ActivatedTimer timerRef;
	Animator animator;

	bool timerEnabled = false;

	void Start(){

		timerRef = transform.parent.GetComponent<ActivatedTimer> ();
		animator = GetComponent<Animator> ();

		// Events
		timerRef.OnTimerStart += StartTimer;
		timerRef.OnTimerRunOut += EndTimer;

		// Init
		GetComponent<Image> ().enabled = false;

	}

	void StartTimer(){
		
		timerEnabled = true;

		if (animator.isInitialized)
			animator.SetBool ("Timer", timerEnabled);

		GetComponent<Image> ().enabled = timerEnabled;

	}

	void EndTimer(){

		timerEnabled = false;

		if (animator.isInitialized)
			animator.SetBool ("Timer", timerEnabled);

		GetComponent<Image> ().enabled = timerEnabled;

	}
}
