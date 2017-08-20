using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// SWITCH SCRIPT TO 'PuzzleButton.cs' INSTEAD



public class WeightButton : MonoBehaviour {
//
//	// Events
//	public delegate void WeightObject_Activate();
//	public event WeightObject_Activate OnWeightButtonActivate;
//
//	public delegate void WeightObject_Deactivate();
//	public event WeightObject_Deactivate OnWeightButtonDeactivate;
//
//	List<GameObject> weightObjects;
//
//	public float targetWeightToUnlock;
//
//	public float currentWeightAchieved = 0;
//
//	bool buttonActivated = false;
//
//	void Start () {
//
//		weightObjects = new List<GameObject> ();
//
//	}
//
//
//	void Update () {
//
//		// If we hit the correct weight the first time...
//		if (currentWeightAchieved >= targetWeightToUnlock && !buttonActivated) {
//
//			if(OnWeightButtonActivate != null)
//				OnWeightButtonActivate ();	// Fire the event cannons!
//			buttonActivated = true;
//
//		} 
//
//		// If we lose the correct weight after having had it...
//		if(currentWeightAchieved < targetWeightToUnlock && buttonActivated){
//
//			if(OnWeightButtonDeactivate != null)
//				OnWeightButtonDeactivate ();	// Fire the event cannons!
//			buttonActivated = false;
//
//		}
//
//	}
//
//
//	void OnTriggerEnter(Collider col){
//
//		if (!weightObjects.Contains (col.gameObject) && col.transform.GetComponent<WeightObject>()) {
//			
//			weightObjects.Add (col.gameObject);
//
//			currentWeightAchieved += col.transform.GetComponent<WeightObject> ().weightValue;
//
//			Debug.Log ("Added: " + col.gameObject.name);
//
//		}
//
//	}
//
//
//	void OnTriggerExit(Collider col){
//
//		if (weightObjects.Contains (col.gameObject) && col.transform.GetComponent<WeightObject>()) {
//
//			weightObjects.Remove (col.gameObject);
//
//			currentWeightAchieved -= col.transform.GetComponent<WeightObject> ().weightValue;
//
//			Debug.Log ("Removed: " + col.gameObject.name);
//
//		}
//
//	}

}
