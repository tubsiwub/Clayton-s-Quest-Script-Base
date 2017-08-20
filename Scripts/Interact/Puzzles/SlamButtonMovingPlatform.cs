using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlamButtonMovingPlatform : MonoBehaviour {

	public GameObject puzzleButton;

	public GameObject newLocation;

	Vector3 oldLocation;

	bool buttonSlammed = false;

	float lerpSpeed;

	WinZone winZone;

	bool winState = false;

	void Start(){

		// References

		// If the button uses a timer, make sure we have a win zone
		if (puzzleButton.GetComponent<PuzzleButton> ().ResetTimer != null) {
			if (GameObject.Find (transform.parent.name + "/WinZone")) {
			
				winZone = GameObject.Find (transform.parent.name + "/WinZone").GetComponent<WinZone> ();

				// Set the events
				winZone.OnWinZoneActivate += Complete;

			} else {

				Debug.Log ("No Winzone Found on - " + transform.name);

			}
		}

		oldLocation = this.transform.position;

		puzzleButton.GetComponent<PuzzleButton>().OnButtonActivated += ButtonSlammed;
		puzzleButton.GetComponent<PuzzleButton>().OnButtonReset += ButtonReset;

	}

	void Complete(){

		winState = true;

	}


	void Update () {

		if (lerpSpeed > 0.1f)
			lerpSpeed -= 0.01f * Time.deltaTime;
		else 
			lerpSpeed = 0.1f;

		if(buttonSlammed)
			transform.position = Vector3.Lerp (transform.position, newLocation.transform.position, lerpSpeed * Time.deltaTime);

	}

	void ButtonSlammed(){

		lerpSpeed = 2;

		buttonSlammed = true;

	}

	void ButtonReset(){

		if (!winState) 
			transform.position = oldLocation;

		buttonSlammed = false;

	}

}
