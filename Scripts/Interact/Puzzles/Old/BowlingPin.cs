using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlingPin : MonoBehaviour {

	// Events
	public delegate void BowlingPin_Knockout();
	public event BowlingPin_Knockout OnPinKnockout;

	// Events
	public delegate void BowlingPin_Touched();
	public event BowlingPin_Touched OnPinTouch;

	[SerializeField]
	float fallTimer;
	float fallTimerDefault = 2.0f;
	//float pinForce = 5.0f;

	Vector3 originPos;
	Quaternion originRot;

	// if false, pin cannot do anything
	bool pinEnabled = false;
	bool markForDeletion = false;
///	bool playerCollided = false;

	GameObject playerObj;
	Rigidbody rb;

	void Start () {

		playerObj = GameObject.FindWithTag ("Player");

		rb = GetComponent<Rigidbody> ();

		if (transform.parent.GetComponent<Bowling_DetectionZone>().bowlingEvent != null) {
			transform.parent.GetComponent<Bowling_DetectionZone>().bowlingEvent.GetComponent<BowlingEvent> ().OnBowlingSuccess += GameOver;
		}

		if (transform.parent.GetComponent<Bowling_DetectionZone>().bowlingStartZone != null) {
			transform.parent.GetComponent<Bowling_DetectionZone>().bowlingStartZone.GetComponent<BowlingStartZone> ().OnZoneEnter += EnablePins;
		}

		fallTimer = fallTimerDefault;

		transform.rotation = Quaternion.Euler (Vector3.zero);

		originPos = transform.position;
		originRot = transform.rotation;

		PinState (false);

		EnablePins (false);

	}

	void EnablePins(){

		EnablePins (true);

	}

	// Only when game starts are the pins enabled
	void EnablePins(bool status){

		pinEnabled = status;
		rb.detectCollisions = status;

	}

	void Update () {

		// Keep the pins from... uh... flying away forever.
		if (GetComponent<Rigidbody> ().velocity.magnitude > 50) {
			GetComponent<Rigidbody> ().velocity = GetComponent<Rigidbody> ().velocity.normalized * 5;
		}

		if (pinEnabled && !markForDeletion) {
			// If the pin is rotated enough to be considered "knocked over", 
			//  	start the timer to ensure that it's not wobbling
			if (Mathf.Abs(transform.up.y) < 0.6f) {

				// mark for deletion
				StartCoroutine(MarkForDeletion());

			}

			// If the pin is not rotated enough to be considered fallen, reset timer
			else {

				fallTimer = fallTimerDefault;

			}
		}

	}

	// True allows the pins to be hit
	public void PinState(bool state){

		if (state) {

			rb.detectCollisions = true;
			rb.useGravity = true;
			rb.isKinematic = false;

		} else {

			rb.detectCollisions = false;
			rb.useGravity = false;
			rb.isKinematic = true;

		}

	}

	// Force-stop the bowling pin events
	void GameOver(){

		StopAllCoroutines ();

		GameObject.FindWithTag ("Player").transform.GetComponent<PlayerHandler> ().SetFrozen (false, true);

	}

	void OnCollisionEnter(Collision col){
		
		if (pinEnabled) {
			if (col.transform.tag == "Player") {
				if (playerObj.GetComponent<BallController> ().GetSpeed > 0.8f) {
					
					//playerCollided = true;

					StartCoroutine (FreezePlayer (0.8f, col.gameObject));

					if (OnPinTouch != null)
						OnPinTouch ();
				}

			}
		}

	}

	public void ResetPin(){

		transform.position = originPos;
		transform.rotation = originRot;
		GetComponent<Rigidbody> ().velocity = Vector3.zero;
		GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
		pinEnabled = true;
		markForDeletion = false;

		StopAllCoroutines ();

	}

	IEnumerator MarkForDeletion(){

		markForDeletion = true;

		while (fallTimer > 0) {
			
			fallTimer -= Time.deltaTime;

			yield return new WaitForEndOfFrame ();

		}

		// Time ran out, definitely fell over.  Remove it after letting the family know.
		if (OnPinKnockout != null) {
			OnPinKnockout ();
		}

		transform.position += -Vector3.up * 1000;
		fallTimer = fallTimerDefault;
		pinEnabled = false;

	}

	IEnumerator FreezePlayer(float delay, GameObject playerObj){

		float counter = delay;
		while (counter > 0) {
		
			playerObj.GetComponent<Rigidbody> ().velocity -= playerObj.GetComponent<Rigidbody> ().velocity / 50;

			counter -= Time.deltaTime;

			yield return new WaitForEndOfFrame ();

		}

		// Is this necessary?  Freeze player while the pins do their thing.
		playerObj.transform.GetComponent<PlayerHandler> ().SetFrozen (true, true);

	}

}
