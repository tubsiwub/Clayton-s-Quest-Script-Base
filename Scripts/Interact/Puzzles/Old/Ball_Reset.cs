using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// accepts a puzzle button that, when pressed, reset a ball to its original position

// place on the push ball if it can be reset

public class Ball_Reset : MonoBehaviour {

	public GameObject puzzleButton;

	Vector3 startingPosition;
	Quaternion startingRotation;
	Vector3 startingScale;


	void Start () {

		// store these values
		startingPosition = this.transform.position;
		startingRotation = this.transform.rotation;
		startingScale = this.transform.localScale;

		if (puzzleButton != null) {
			puzzleButton.GetComponent<PuzzleButton> ().OnButtonActivated += ResetBall;
		}

	}

	void ResetBall(){

		// Reset ball to orig. values
		this.transform.position = startingPosition;
		this.transform.rotation = startingRotation;
		this.transform.localScale = startingScale;

		this.GetComponent<Rigidbody> ().velocity = Vector3.zero;

		StartCoroutine (ResetButton ());

	}

	IEnumerator ResetButton(){

		print ("4");
		yield return new WaitForSeconds (0.4f);

		if(puzzleButton)
			puzzleButton.GetComponent<PuzzleButton> ().ButtonReset ();

	}

	void OnTriggerEnter(Collider col){
		if (col.tag == "Water") {
			ResetBall ();
		}
	}

}
