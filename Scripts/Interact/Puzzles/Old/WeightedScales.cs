using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightedScales : MonoBehaviour {

	List<GameObject> weightObjects;

	public List<GameObject> WeightObjects { get { return weightObjects; } }

	public GameObject otherScale;

	// Scale sinks at certain value
	public float targetWeight;

	public float currentWeight = 0;

	// For now, these positions will be entered manually
	public Vector3 
		centerHeight;

	public Vector3 
		highPoint,
		lowPoint;

	void Start () {

		weightObjects = new List<GameObject> ();

		// Events
		//otherScale.GetComponent<WeightedScales>().OnWeightScaleUp += 

	}


	void Update () {

//		if (currentWeight >= targetWeight &&
//			currentWeight > otherScale.GetComponent<WeightedScales>().currentWeight &&
//			transform.localPosition != lowPoint) {
		if (currentWeight >= targetWeight &&
			currentWeight > otherScale.GetComponent<WeightedScales>().currentWeight &&
			(transform.localPosition != lowPoint || 
				otherScale.transform.localPosition != otherScale.GetComponent<WeightedScales>().highPoint)) {
			
			transform.localPosition = Vector3.MoveTowards (transform.localPosition, lowPoint, 2 * Time.deltaTime);
			otherScale.transform.localPosition = Vector3.MoveTowards (otherScale.transform.localPosition, otherScale.GetComponent<WeightedScales>().highPoint, 2 * Time.deltaTime);
		
			// Cycle through objects and shift them by the amount moved
			foreach (GameObject obj in weightObjects) {
				//Vector3 newPos = centerHeight;
				obj.transform.localPosition = Vector3.MoveTowards (obj.transform.localPosition, lowPoint, 2 * Time.deltaTime);
			}

			// Cycle through objects and shift them by the amount moved
			foreach (GameObject obj in otherScale.GetComponent<WeightedScales>().WeightObjects){
				//Vector3 newPos = centerHeight;
				obj.transform.localPosition = Vector3.MoveTowards (obj.transform.localPosition, highPoint, 2 * Time.deltaTime);
			}

		}

		if (currentWeight == otherScale.GetComponent<WeightedScales> ().currentWeight &&
		   transform.localPosition != centerHeight) {

			transform.localPosition = Vector3.MoveTowards (transform.localPosition, centerHeight, 2 * Time.deltaTime);

			// Cycle through objects and shift them by the amount moved
			foreach (GameObject obj in weightObjects) {
				Vector3 newPos = centerHeight;
				obj.transform.localPosition = Vector3.MoveTowards (obj.transform.localPosition, newPos, 2 * Time.deltaTime);
			}

			// Cycle through objects and shift them by the amount moved
			foreach (GameObject obj in otherScale.GetComponent<WeightedScales>().WeightObjects){
				Vector3 newPos = centerHeight;
				obj.transform.localPosition = Vector3.MoveTowards (obj.transform.localPosition, newPos, 2 * Time.deltaTime);
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

	// Remove stored weight and object when object leaves trigger
//	void OnTriggerExit(Collider col){
//
//		if (weightObjects.Contains (col.gameObject) && col.transform.GetComponent<WeightObject>()) {
//
//			weightObjects.Remove (col.gameObject);
//
//			currentWeight -= col.transform.GetComponent<WeightObject> ().weightValue;
//
//			Debug.Log ("Removed: " + col.gameObject.name);
//
//		}
//
//	}

}
