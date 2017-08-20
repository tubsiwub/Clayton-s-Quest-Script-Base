using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlingEvent : MonoBehaviour {

	// Events
	public delegate void BowlingEventSuccess();
	public event BowlingEventSuccess OnBowlingSuccess;

	public GameObject pinsContainer;


	List<GameObject> bowlingPins;

	int 
	winCount, 
	currentCount = 0;

	float 
	resetTimer,
	resetTimerDefault = 6.0f;

	GameObject
	playerObj;

	public GameObject
	timerObj;

	public GameObject 
	startPosition;

	bool 
	winGame = false,
	gameStarted = false;

	public bool
	TimerStatus = false;

	void Start () {

		if (timerObj != null) {

			timerObj.GetComponent<ActivatedTimer> ().OnTimerStart += TimerStart;
			timerObj.GetComponent<ActivatedTimer> ().OnTimerRunOut += TimerRunOut;

		}

		if (pinsContainer != null) {

			pinsContainer.GetComponent<Bowling_DetectionZone> ().OnZoneEnter += BowlingZoneSuccess;

		}

		bowlingPins = new List<GameObject> ();

		foreach (Transform child in pinsContainer.transform)
			bowlingPins.Add (child.gameObject);

		resetTimer = resetTimerDefault;

		playerObj = GameObject.FindWithTag ("Player");

		winCount = bowlingPins.Count;

		foreach (GameObject pin in bowlingPins) {

			pin.GetComponent<BowlingPin> ().OnPinKnockout += AddToCount;
			pin.GetComponent<BowlingPin> ().OnPinTouch += StartGame;

		}
	
	}

	void BowlingZoneSuccess(){

		foreach (GameObject pin in bowlingPins) {

			pin.GetComponent<BowlingPin> ().PinState (true);
			pin.GetComponent<BowlingPin> ().ResetPin ();

		}

	}

	void AddToCount(){

		Debug.Log ("Adding to count: " + currentCount + " + 1");

		currentCount += 1;

		if (resetTimer < 3.0f) {
			resetTimer += 1.0f;
		}

	}

	void Update () {

		// If the player isn't a ball, cancel the game
		if (playerObj.GetComponent<PlayerHandler> ().CurrentState == PlayerHandler.PlayerState.Human) {

			TimerStatus = false;

			timerObj.GetComponent<ActivatedTimer> ().EndTimer ();

		}

		if (!winGame && gameStarted) {

			if (playerObj.GetComponent<PlayerHandler> ().IsFrozen ) {
				playerObj.GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
				playerObj.GetComponent<Rigidbody> ().velocity = Vector3.zero;

				RaycastHit hit;
				if (Physics.Raycast (playerObj.transform.position, -Vector3.up, out hit)) {
					playerObj.transform.position = hit.point + Vector3.up * playerObj.GetComponent<CapsuleCollider>().height / 2;
				}
			}

			if (currentCount >= winCount) {

				Debug.Log ("Win Game");

				winGame = true;

				// Tell the timer to finish
				timerObj.GetComponent<ActivatedTimer> ().Complete ();

				//	unfreeze and move player back to the start pad
				playerObj.transform.position = startPosition.transform.position;
				playerObj.transform.GetComponent<PlayerHandler>().SetFrozen(false,true);
				StopAllCoroutines ();

				// win?  I dunno.
				if (OnBowlingSuccess != null) {
					OnBowlingSuccess ();
				}

			}

			Debug.Log ("Current Count: " + currentCount);

			// Count down until the pins reset
			resetTimer -= Time.deltaTime;

			if (resetTimer <= 0) {

				Debug.Log ("Reset Game");

				ResetBowlingGame ();

			}

		}

		#if UNITY_EDITOR
		if (Input.GetKeyDown (KeyCode.L)) {
			
			TimerStatus = false;

		}
		#endif

	}

	void StartGame(){

		gameStarted = true;

		TimerStatus = false;

		// Tell the timer to reset
		timerObj.GetComponent<ActivatedTimer> ().EndTimer();

	}

	void TimerStart(){

		TimerStatus = true;

	}

	void TimerRunOut(){

		// If the timer is still enabled, reset the game when the timer runs out
		if (TimerStatus)
			ResetBowlingGame ();

		TimerStatus = false;

	}

	void ResetBowlingGame(){

		if (!winGame) {
			
			// Game Reset somehow:
			gameStarted = false;
			winGame = false;

			//	move pins back to original spots and reset rotations
			foreach (GameObject pin in bowlingPins) {
				pin.GetComponent<BowlingPin> ().ResetPin ();
			}

			//	reset variables, counters and timers
			currentCount = 0;
			resetTimer = resetTimerDefault;

			//	unfreeze and move player back to the start pad
			playerObj.transform.GetComponent<PlayerHandler> ().SetFrozen (false, true);
			playerObj.transform.position = startPosition.transform.position;
			playerObj.GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
			playerObj.GetComponent<Rigidbody> ().velocity = Vector3.zero;

		}

	}

	// If player leaves the play area, exit the game
	void OnTriggerExit(Collider col){

		if (col.transform.tag == "Player") {

			TimerStatus = false;

			timerObj.GetComponent<ActivatedTimer> ().EndTimer ();

		}

	}
		

}
