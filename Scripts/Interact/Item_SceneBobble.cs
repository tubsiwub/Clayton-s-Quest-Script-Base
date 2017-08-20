using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_SceneBobble : MonoBehaviour {

	public float tiltMaxAngle, tiltTiltSpeed;
	public float rotateRotSpeed;
	public float bobMaxHeight, bobShiftSpeed;

	void Enable() {
		
		StartCoroutine (TiltAngle(tiltMaxAngle, tiltTiltSpeed));
		StartCoroutine (RotateInCircle(rotateRotSpeed));
		StartCoroutine (BobUpAndDown(bobMaxHeight, bobShiftSpeed));

	}

	void Start () {

		StartCoroutine (TiltAngle(tiltMaxAngle, tiltTiltSpeed));
		StartCoroutine (RotateInCircle(rotateRotSpeed));
		StartCoroutine (BobUpAndDown(bobMaxHeight, bobShiftSpeed));

	}

	void OnEnable () {

		Enable();
	}

	void OnDisable () {

		StopAllCoroutines();
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
		StartCoroutine (TiltAngle(tiltMaxAngle, tiltTiltSpeed));
	}



	// Spins the bottle endlessly in a circle
	IEnumerator RotateInCircle(float rotateSpeed){

		while (true) {

			transform.Rotate (0, rotateRotSpeed * Time.deltaTime, 0);

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
		StartCoroutine (BobUpAndDown(bobMaxHeight, bobShiftSpeed));
	}

}

