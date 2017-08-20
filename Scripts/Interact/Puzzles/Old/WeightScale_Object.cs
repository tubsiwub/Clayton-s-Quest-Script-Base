using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Contains code allowing an object to listen for scale events and react accordingly


public class WeightScale_Object : MonoBehaviour {


	public GameObject weightScaleMain;

	public Vector3 leftPosition, balancedPosition, rightPosition;

	public float shiftSpeed;


	void Start () {

		weightScaleMain.GetComponent<WeightScale_Main> ().OnScaleBalanced += Balanced;
		weightScaleMain.GetComponent<WeightScale_Main> ().OnScaleLeftHeavy += LeftHeavy;
		weightScaleMain.GetComponent<WeightScale_Main> ().OnScaleRightHeavy += RightHeavy;

	}

	void Balanced(){
		StopAllCoroutines ();
		StartCoroutine (ShiftPosition (balancedPosition));
	}

	void LeftHeavy(){
		StopAllCoroutines ();
		StartCoroutine (ShiftPosition (leftPosition));
	}

	void RightHeavy(){
		StopAllCoroutines ();
		StartCoroutine (ShiftPosition (rightPosition));
	}

	IEnumerator ShiftPosition(Vector3 newPos){

		// While not close to new position, slowly shift it to it's new position
		while(Vector3.Distance(transform.position, newPos) > 0.1f){

			transform.position = Vector3.MoveTowards (transform.position, newPos, shiftSpeed * Time.deltaTime);

			yield return new WaitForEndOfFrame();

		}

		// Set to newPos when close enough
		transform.position = newPos;

	}

}
