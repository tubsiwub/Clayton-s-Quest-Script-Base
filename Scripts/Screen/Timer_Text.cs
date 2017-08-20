using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer_Text : MonoBehaviour {

	ActivatedTimer timerRef;

	bool timerEnabled = false;

	float timeRemaining;

	void Start () {

		timerRef = transform.parent.GetComponent<ActivatedTimer> ();

		// Events
		timerRef.OnTimerStart += StartTimer;
		timerRef.OnTimerRunOut += EndTimer;

		// Init
		GetComponent<Text> ().enabled = false;

	}


	void Update () {

		if (timerEnabled) {

			int minutes = (int)Mathf.Ceil (timerRef.TimeRemaining) / 60;
			int seconds = (int)Mathf.Ceil (timerRef.TimeRemaining) % 60;


			if(minutes < 10 && minutes > 0)
				GetComponent<Text> ().text = "0" + minutes.ToString ();
			else 
				GetComponent<Text> ().text = minutes.ToString ();

			if(seconds < 10)
				GetComponent<Text> ().text += ":0" + seconds.ToString ();
			else 
				GetComponent<Text> ().text += ":" + seconds.ToString ();

		}

	}


	void StartTimer(){

		timerEnabled = true;

		GetComponent<Text> ().enabled = timerEnabled;

	}

	void EndTimer(){

		timerEnabled = false;

		GetComponent<Text> ().enabled = timerEnabled;

	}
}
