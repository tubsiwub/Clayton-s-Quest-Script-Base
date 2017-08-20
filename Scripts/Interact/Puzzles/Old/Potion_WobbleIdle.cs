using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion_WobbleIdle : MonoBehaviour {

	public float tiltMaxAngle, tiltTiltSpeed;
	public float rotateRotSpeed;
	public float bobMaxHeight, bobShiftSpeed;

	public ParticleSystem splashParticleSystem;

	Animator anim;

	void Start () {

		anim = GetComponent<Animator> ();

		StartCoroutine (TiltAngle(tiltMaxAngle, tiltTiltSpeed));
		StartCoroutine (RotateInCircle(rotateRotSpeed));
		StartCoroutine (BobUpAndDown(bobMaxHeight, bobShiftSpeed));

	}



	void Update () {

		if (Input.GetKeyDown (KeyCode.Q))
			HealthManager.instance.LoseALife ();

	}




	// Slowly tilt the bottle between straight and tilted
	IEnumerator TiltAngle(float maxAngle, float tiltSpeed){

		float counter = -maxAngle;

		// tilt until the correct angle
		while (counter < maxAngle) {

			counter += tiltSpeed * Time.deltaTime;

			transform.rotation = Quaternion.Euler (
				new Vector3 (
					counter,	// tilt it slowly
					transform.rotation.eulerAngles.y,
					0));

			yield return new WaitForEndOfFrame ();
		}

		// one final nudge into the correct angle
		// ==
		counter = maxAngle;

		transform.rotation = Quaternion.Euler (
			new Vector3 (
				counter,	// tilt it slowly
				transform.rotation.eulerAngles.y,
				0));
		
		yield return new WaitForEndOfFrame ();
		// ==

		// tilt back to zero before repeating
		while (counter > -maxAngle) {

			counter -= tiltSpeed * Time.deltaTime;

			transform.rotation = Quaternion.Euler (
				new Vector3 (
					counter,	// tilt it slowly
					transform.rotation.eulerAngles.y,
					0));

			yield return new WaitForEndOfFrame ();
		}

		// one final nudge into the correct angle
		// ==
		counter = -maxAngle;

		transform.rotation = Quaternion.Euler (
			new Vector3 (
				counter,	// tilt it slowly
				transform.rotation.eulerAngles.y,
				0));
		
		yield return new WaitForEndOfFrame ();
		// ==

		// Restart for endless loop
		StartCoroutine (TiltAngle(maxAngle, tiltSpeed));
	}



	// Spins the bottle endlessly in a circle
	IEnumerator RotateInCircle(float rotateSpeed){

		while (true) {
			
			transform.Rotate (0, rotateSpeed * Time.deltaTime, 0);

			yield return new WaitForEndOfFrame ();

		}

	}



	// Bob up and down while idling
	IEnumerator BobUpAndDown(float maxHeight, float shiftSpeed){

		float storedHeight = transform.position.y;

		float counter = transform.position.y;

		while (counter < storedHeight + maxHeight) {

			counter += shiftSpeed * Time.deltaTime;

			transform.position = new Vector3 (
				transform.position.x,
				counter,
				transform.position.z);

			yield return new WaitForEndOfFrame ();

		}

		// one final nudge into the correct angle
		// ==
		counter = storedHeight + maxHeight;

		transform.position = new Vector3 (
			transform.position.x,
			counter,
			transform.position.z);

		yield return new WaitForEndOfFrame ();
		// ==

		while (counter > storedHeight) {

			counter -= shiftSpeed * Time.deltaTime;

			transform.position = new Vector3 (
				transform.position.x,
				counter,
				transform.position.z);

			yield return new WaitForEndOfFrame ();

		}

		// one final nudge into the correct angle
		// ==
		counter = storedHeight;

		transform.position = new Vector3 (
			transform.position.x,
			counter,
			transform.position.z);

		yield return new WaitForEndOfFrame ();
		// ==

		// Restart for endless loop
		StartCoroutine (BobUpAndDown(maxHeight, shiftSpeed));
	}

	void OnTriggerEnter(Collider col){

		if(col.transform.tag == "Player")
			CollectBottle (col.transform.gameObject);

	}

	void CollectBottle(GameObject playerObj){

		// heal player
		HealthManager.instance.RegainLives (1);

		// spawn emitter / effects
		StartCoroutine (SplashEmitter ());

	}

	IEnumerator SplashEmitter(){

		splashParticleSystem.transform.parent = null;
		splashParticleSystem.Play ();

		anim.SetTrigger ("Collect");

		this.GetComponent<Collider> ().enabled = false;

		yield return new WaitForSeconds (2.0f);

		// destroy bottle
		Destroy(splashParticleSystem.gameObject);
		Destroy (this.gameObject);

	}

}
