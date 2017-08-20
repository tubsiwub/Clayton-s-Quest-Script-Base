using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Contains code to allow a set of two scales to affect one another when weight objects are used


// which scale is this?
public enum SCALE {

	LEFT, RIGHT

}

public class WeightScale_Scale : MonoBehaviour {

	public SCALE scaleSide = SCALE.LEFT;

	List<GameObject> weightObjects;

	public List<GameObject> WeightObjects { get { return weightObjects; } }

	public GameObject otherScale;

	// Scale sinks at certain value
	public float targetWeight;

	public float currentWeight = 0;

	Vector3 lastPosition;

	Animator anim;

	// Toggle to allow scales to fire events only once
	bool balanced = true;

	void Start () {

		// Get the scale animator from the parent
		anim = transform.parent.GetComponent<Animator> ();

		weightObjects = new List<GameObject> ();

		lastPosition = transform.position;

	}


	void Update () {

		Vector3 changeInPosition = Vector3.zero;

		if (lastPosition != transform.position) {
			changeInPosition = transform.position - lastPosition;
		}

		lastPosition = transform.position;

		// If this scale weighs more than the other scale
		if (currentWeight >= targetWeight &&
			currentWeight > otherScale.GetComponent<WeightScale_Scale>().currentWeight &&
			balanced) {

			balanced = false;

			// Shift scales
			if (scaleSide == SCALE.LEFT && anim.GetCurrentAnimatorStateInfo (0).IsName ("WeightScale_BalancedIdle")) 
			{ 
				anim.ResetTrigger ("BalanceScale");
				anim.SetTrigger ("TiltLeft"); 

				// Tell parent to fire the event
				transform.parent.GetComponent<WeightScale_Main> ().FireEvent ("left");

			}
			if (scaleSide == SCALE.RIGHT && anim.GetCurrentAnimatorStateInfo (0).IsName ("WeightScale_BalancedIdle")) 
			{ 
				anim.ResetTrigger ("BalanceScale");
				anim.SetTrigger ("TiltRight");

				// Tell parent to fire the event
				transform.parent.GetComponent<WeightScale_Main> ().FireEvent ("right");

			}

			// Cycle through objects and shift them by the amount moved
			foreach (GameObject obj in weightObjects) {
				obj.transform.position += changeInPosition;
			}

			// Cycle through objects and shift them by the amount moved for other scale
			foreach (GameObject obj in otherScale.GetComponent<WeightScale_Scale>().WeightObjects){
				obj.transform.position += changeInPosition;
			}

		}

		// If both scale weights are equal
		if (currentWeight == otherScale.GetComponent<WeightScale_Scale> ().currentWeight &&
			!balanced) {

			balanced = true;

			// Tell parent to fire the event
			transform.parent.GetComponent<WeightScale_Main> ().FireEvent ("balanced");

			// Shift scales
			if (anim.GetCurrentAnimatorStateInfo (0).IsName ("WeightScale_LeftWeightedIdle") ||
			   anim.GetCurrentAnimatorStateInfo (0).IsName ("WeightScale_RightWeightedIdle")) 
			{ 
				anim.ResetTrigger ("TiltRight");
				anim.ResetTrigger ("TiltLeft");
				anim.SetTrigger ("BalanceScale"); 
			}

			// Cycle through objects and shift them by the amount moved
			foreach (GameObject obj in weightObjects) {
				obj.transform.position += changeInPosition;
			}

			// Cycle through objects and shift them by the amount moved for other scale
			foreach (GameObject obj in otherScale.GetComponent<WeightScale_Scale>().WeightObjects){
				obj.transform.position += changeInPosition;
			}

		}


		foreach (GameObject obj in weightObjects) {

			if (Vector3.Distance (obj.transform.position, transform.position) > 2) {

				weightObjects.Remove (obj);

				currentWeight -= obj.transform.GetComponent<WeightObject> ().weightValue;

				break;

			}

		}


	}

	// Stores object and its weight value when an accepted object enters the trigger field
	void OnTriggerEnter(Collider col){

		if (!weightObjects.Contains (col.gameObject) && col.transform.GetComponent<WeightObject>()) {

			weightObjects.Add (col.gameObject);

			currentWeight += col.transform.GetComponent<WeightObject> ().weightValue;

		}

	}

}

