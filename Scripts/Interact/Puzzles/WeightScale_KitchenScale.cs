using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightScale_KitchenScale : MonoBehaviour {

	public List<GameObject> weightObjects;

	public float targetWeight;
	public float currentWeight = 0;

	public bool lockObjects = true;	// prevent objects from leaving after being registered on the weight scale

	public List<GameObject> otherObject;
	public float otherObjectSpeed;
	public List<Vector3> otherObjectShiftPos;
	List<Vector3> otherObjectOrigPos;

	Transform scaleDish;
	Transform scaleRod;

	Vector3 rodStartPos, rodShiftPos;	// local values

	//Animator anim;

	string storageKey = "";

	void Awake () {
		
		weightObjects = new List<GameObject> ();

		//anim = GetComponent<Animator> ();
		scaleDish = transform.GetChild(2).GetChild(0);

		otherObjectOrigPos = new List<Vector3> ();
		for(int i = 0; i < otherObject.Count; i++){
			
			otherObjectOrigPos.Add(otherObject[i].transform.position);
		}

		scaleDish = transform.GetChild (2).GetChild (0);
		scaleRod = transform.GetChild (2);

		rodStartPos = scaleRod.transform.localPosition;
		rodShiftPos = new Vector3 (rodStartPos.x, 0.2364f, rodStartPos.z);

		// Event Triggers
		if (GetComponent<SavingLoading_StorageKeyCheck> ()) {
			GetComponent<SavingLoading_StorageKeyCheck> ().OnKeyCheck += KeyCheck;
			storageKey = GetComponent<SavingLoading_StorageKeyCheck> ().storageKey;
		}
	}

	bool complete = false;

	void Update () {

		if (complete)	// if we finish, then we're done - logical.
			return;

		if (currentWeight >= targetWeight)
		{
			StartCoroutine (SetComplete ());
			GetComponent<SavingLoading_StorageKeyCheck> ().OnKeyCheck -= KeyCheck;
			GetComponent<SavingLoading_StorageKeyCheck> ().enabled = false;
			SavingLoading.instance.SaveStorageKey (storageKey, true);
		}

		// Get Weight Percentage
		float weightPercentage = 0;
		if (currentWeight > 0) {
			weightPercentage = MathFunctions.ConvertNumberRanges (currentWeight, targetWeight, 0, 1.0f, 0);
		}
		if (weightPercentage > 1.0f) // Don't scale above 100%
		{	
			weightPercentage = 1.0f;
		}

		Vector3 rodNewPos = MathFunctions.PositionBetweenTwoVector3 (rodStartPos, rodShiftPos, weightPercentage);

		// Shift scaleRod toward destination
		if (Vector3.Distance (scaleRod.localPosition, rodNewPos) > 0.01f) {
			scaleRod.localPosition = Vector3.MoveTowards (scaleRod.localPosition, rodNewPos, 0.1f * Time.deltaTime);
		} else {
			scaleRod.localPosition = rodNewPos;
		}

		for(int oo = 0; oo < otherObject.Count; oo++) {
			
			Vector3 newPosition = MathFunctions.PositionBetweenTwoVector3 (otherObjectOrigPos [oo], otherObjectShiftPos [oo], weightPercentage);

			if (currentWeight > 0) {

				// Shift otherObject toward destination
				if (Vector3.Distance (otherObject [oo].transform.position, newPosition) > 0.01f) {
					otherObject [oo].transform.position = Vector3.MoveTowards (otherObject [oo].transform.position, newPosition, otherObjectSpeed * Time.deltaTime);
				} else {
					otherObject [oo].transform.position = newPosition;
				}

			} 
		}


		// Check to see if list objects are still on the scale
		foreach (GameObject obj in weightObjects) {

			if (obj != null)
			{
				// Remove if...
				if (Vector3.Distance (transform.position, obj.transform.position) > (transform.localScale.x * 400))
				{
					print ("Removed");
					if (weightObjects.Contains (obj.gameObject))
					{
						weightObjects.Remove (obj.gameObject);
						UpdateWeight ();
						break;
					}
				} 

				// Remove if...
				if (obj.transform.position.y < transform.position.y + 1)
				{
					if (weightObjects.Contains (obj.gameObject))
					{
						weightObjects.Remove (obj.gameObject);
						UpdateWeight ();
						break;
					}
				}

				// Stick the weights to the surface
				Vector3 newPos = obj.transform.position;
				newPos.y = scaleDish.position.y;
				obj.transform.position = newPos;
			}
		}
	}

	IEnumerator SetComplete(){

		yield return new WaitForSeconds(2.0f);

		complete = true;

	}

	// If storageKey marks this as completed, perform these actions
	void KeyCheck(){
		
		GetComponent<SavingLoading_StorageKeyCheck> ().OnKeyCheck -= KeyCheck;

		// prevents the update function
		complete = true;

		// shifts scale rod to final position
		Vector3 rodNewPos = MathFunctions.PositionBetweenTwoVector3 (rodStartPos, rodShiftPos, 1.0f);
		scaleRod.localPosition = rodNewPos;

		// Sets otherObjects to final positions
		for (int oo = 0; oo < otherObject.Count; oo++)
		{
			Vector3 newPosition = MathFunctions.PositionBetweenTwoVector3 (otherObjectOrigPos [oo], otherObjectShiftPos [oo], 1.0f);
			otherObject [oo].transform.position = newPosition;
		}

		float numberOfWeights = targetWeight / 50;

		GameObject[] paperWeights = new GameObject[(int)numberOfWeights];

		float anglePosition = 360 / numberOfWeights;

		for (int i = 0; i < numberOfWeights; i++)
		{
			paperWeights [i] = (GameObject)Instantiate (Resources.Load<GameObject> ("PaperWeight"), scaleDish.transform.position, Quaternion.Euler (Vector3.zero));

			Vector3 storedRotation = paperWeights [i].transform.rotation.eulerAngles;
			storedRotation.y += anglePosition * i;
			paperWeights [i].transform.rotation = Quaternion.Euler (storedRotation);
			paperWeights[i].transform.position = scaleDish.transform.position + paperWeights[i].transform.forward * 2;

			if (paperWeights [i].GetComponent<Pickupable> ())
				Destroy (paperWeights [i].GetComponent<Pickupable> ());

			if (paperWeights [i].GetComponent<ObjInfo> ())
				Destroy (paperWeights [i].GetComponent<ObjInfo> ());

			if (paperWeights [i].GetComponent<Rigidbody> ())
				Destroy (paperWeights [i].GetComponent<Rigidbody> ());

			weightObjects.Add (paperWeights [i]);
		}

		if (weightObjects.Count > targetWeight/50 && weightObjects.Count > 1)
		{
			for (int i = weightObjects.Count; i > targetWeight/50; i--)
			{
				GameObject storedObj = weightObjects [i - 1];
				weightObjects.Remove (weightObjects [i-1]);
				Destroy (storedObj);
				print ("destroyed");
			}
		}
	}

	void UpdateWeight(){

		currentWeight = 0;
		foreach (GameObject obj in weightObjects) {
				currentWeight += obj.GetComponent<WeightObject> ().weightValue;
		}
	}

	void OnTriggerEnter(Collider col){

		if (col.GetComponent<WeightObject>() 
			&& col.GetComponent<ObjInfo>()) 
		{
			if (!weightObjects.Contains (col.gameObject) && currentWeight < targetWeight) 
			{
				weightObjects.Add (col.gameObject);
				col.transform.position += Vector3.up * 2;
				col.GetComponent<ObjInfo> ().SAVE (true, true);
				SavingLoading.instance.SaveData ();
				UpdateWeight ();
			}

			if (lockObjects) 
			{
				// Prevent removal by player pickup
				if (col.GetComponent<Pickupable> ())
					col.GetComponent<Pickupable> ().enabled = false;
		
				// Prevent collisions
				if (col.GetComponent<Rigidbody> ()) {
					col.GetComponent<Rigidbody> ().isKinematic = true;
					col.GetComponent<Rigidbody> ().useGravity = false;
				}

				// Snap objects to the scale in some way
				Ray ray = new Ray (col.transform.position, -Vector3.up);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit)) {
					if(hit.transform.name == "Scale_Dish")
						col.transform.position = hit.point;
				}
			}
		}
	}

	void OnTriggerStay(Collider col){

		if (col.transform.GetComponent<WeightObject>()) {
			if (!weightObjects.Contains (col.gameObject) && currentWeight < targetWeight) {

				print (col.name);

				weightObjects.Add (col.gameObject);
				UpdateWeight ();
			}
		}
	}
}
