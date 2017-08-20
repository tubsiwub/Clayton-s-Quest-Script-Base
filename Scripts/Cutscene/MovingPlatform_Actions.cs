using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform_Actions : MonoBehaviour {

	[Tooltip("Specific location the platform will move to.")]

	public Vector3 openLocation;
	public Vector3 openPosTemp;
	Vector3 closedLocation;

	float delay = 2.0f;

	void Awake(){

		closedLocation = transform.localPosition;

		INITIALIZE (0);
	}

	public void INITIALIZE (float delayTime) {

//		var theTrace = new System.Diagnostics.StackTrace();
//		Debug.Log(theTrace.GetFrame(1).GetMethod().Name);

		delay = delayTime;

		transform.localPosition = closedLocation;

	}

	void Update () {

		if(delay > -1)
			delay -= Time.deltaTime;

		if(delay <= 0)
			transform.localPosition = Vector3.Lerp (transform.localPosition, openLocation, 2 * Time.deltaTime);

	}


	// temp storage
	public float x, y, z;


}

