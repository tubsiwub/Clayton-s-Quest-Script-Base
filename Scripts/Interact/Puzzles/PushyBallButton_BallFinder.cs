using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushyBallButton_BallFinder : MonoBehaviour {

	public GameObject puzzleButton;

	public GameObject respawnPoint;

	void Start () {
		
	}


	void OnTriggerEnter (Collider col) {

		// if it's a pushyball and it's not already set, add it to the button.
		if (col.GetComponent<PushyBall> () && puzzleButton.GetComponent<PuzzleButton>().keyObj != col.gameObject) {
			
			puzzleButton.GetComponent<PuzzleButton> ().SetBall (col.gameObject, respawnPoint);
			puzzleButton.GetComponent<Animator> ().SetTrigger ("Reset");
			puzzleButton.GetComponent<Animator> ().ResetTrigger ("Push");

		}

	}

}
