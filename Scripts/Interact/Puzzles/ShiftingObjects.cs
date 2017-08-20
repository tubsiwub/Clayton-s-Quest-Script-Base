using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script to allow multiple objects to activate one another, one at a time

public class ShiftingObjects : MonoBehaviour {


	// Events
	public delegate void FireEvents();
	public event FireEvents OnFireEvents;	// when this object finishes / becomes inactive

	public delegate void ListenerActivate();
	public event ListenerActivate OnListenerActivate;	// when this becomes active


	// Important bool:  Determines if the object executes commands or not.
	public bool ACTIVE = false;

	// editor bools
	public bool tutorial;

	public bool FIRST;

	public bool multipleListeners;


	public GameObject listener;

	public List<GameObject> listeners;

	public GameObject puzzleButton;


	public float delayTime;

	// every time a listener is activated, tick this up
	int listenersActiveCount;


	// POSITION, ROTATION, SCALE
	public Vector3 inactivePosition;
	public Vector3 activePosition;

	public Quaternion inactiveRotation;
	public Quaternion activeRotation;

	public Vector3 inactiveScale;
	public Vector3 activeScale;

	Vector3 tempEuler;

	// TRANSITION SPEEDS
	public float transitionPosition;
	public float transitionRotation;
	public float transitionScale;

	void Start () {

		if(listener != null && !multipleListeners)
		listener.GetComponent<ShiftingObjects> ().OnFireEvents += SetActive;

		if (multipleListeners) {

			foreach (GameObject obj in listeners) {

				if (obj != null)
					obj.GetComponent<ShiftingObjects> ().OnFireEvents += CountActive;

			}
		
		}

		puzzleButton.GetComponent<PuzzleButton> ().OnButtonActivated += ButtonActivated;

		if (FIRST)
			SetActive ();

	}

	// make sure all listeners have been activated before starting
	void CountActive(){

		int listenerCount = 0;

		foreach (GameObject obj in listeners)
			if (obj != null)
				listenerCount += 1;

		if (listenersActiveCount < listenerCount - 1)
			listenersActiveCount += 1;
		else
			SetActive ();

	}

	// start this object
	void SetActive(){

		// fire event for scripts that aren't in the queue
		if (OnListenerActivate != null)
			OnListenerActivate ();

		StartCoroutine (SetActive_IEnum ());

	}

	// waits a period of time before setting object as active (useful for cutscenes)
	IEnumerator SetActive_IEnum(){

		float counter = delayTime;

		while (counter > 0) {

			counter -= Time.deltaTime;

			yield return new WaitForEndOfFrame ();

		}

		ACTIVE = true;

	}

	void ButtonActivated(){

		if (OnFireEvents != null)
			OnFireEvents ();

		if (FIRST)
			FIRST = false;

		ACTIVE = false;

	}

	void Update () {

		if (ACTIVE) {
			
			if (Vector3.Distance (transform.position, activePosition) > 0.00001f) {
				transform.position = Vector3.MoveTowards (transform.position, activePosition, transitionPosition * Time.deltaTime);	// 1.0, 8.0, 20.0
			} else
				transform.position = activePosition;

			if (Mathf.Abs(Vector3.Distance (transform.rotation.eulerAngles, activeRotation.eulerAngles)) > 0.00001f) {
				tempEuler = Vector3.RotateTowards (transform.rotation.eulerAngles, activeRotation.eulerAngles, 0.1f * Time.deltaTime, transitionRotation * Time.deltaTime);	// 10, 100, 400
				transform.rotation = Quaternion.Euler (tempEuler);
			} else
				transform.rotation = Quaternion.Euler (activeRotation.eulerAngles);

			if (Vector3.Distance (transform.localScale, activeScale) > 0.00001f) {
				transform.localScale = Vector3.Lerp (transform.localScale, activeScale, transitionScale * Time.deltaTime);		// 0.2, 1.0, 5.0
			} else
				transform.localScale = activeScale;

		} else {

			if (Vector3.Distance (transform.position, inactivePosition) > 0.00001f) {
				transform.position = Vector3.MoveTowards (transform.position, inactivePosition, transitionPosition * Time.deltaTime);
			} else
				transform.position = inactivePosition;

			if (Mathf.Abs(Vector3.Distance (transform.rotation.eulerAngles, inactiveRotation.eulerAngles)) > 0.00001f) {
				tempEuler = Vector3.RotateTowards (transform.rotation.eulerAngles, inactiveRotation.eulerAngles, 0.1f * Time.deltaTime, transitionRotation * Time.deltaTime);
				transform.rotation = Quaternion.Euler (tempEuler);
			} else
				transform.rotation = Quaternion.Euler (inactiveRotation.eulerAngles);

			if (Vector3.Distance (transform.localScale, inactiveScale) > 0.00001f) {
				transform.localScale = Vector3.Lerp (transform.localScale, inactiveScale, transitionScale * Time.deltaTime);
			} else
				transform.localScale = inactiveScale;

		}

	}
}

