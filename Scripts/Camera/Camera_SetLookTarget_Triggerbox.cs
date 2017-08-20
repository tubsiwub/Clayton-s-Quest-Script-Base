using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_SetLookTarget_Triggerbox : MonoBehaviour {


	public GameObject targetLocation;

	public float lookSpeed;

	void Start () {

	}




	void Update () {
		
	}


	void OnTriggerEnter(Collider col){

		if(col.transform.tag == "Player")
			Camera.main.GetComponent<CameraControlDeluxe> ().SetLookTarget (targetLocation.transform.position, lookSpeed * Time.deltaTime);

	}

	void OnTriggerExit(Collider col){

		if(col.transform.tag == "Player")
			Camera.main.GetComponent<CameraControlDeluxe> ().CancelLookTarget ();

	}

}
