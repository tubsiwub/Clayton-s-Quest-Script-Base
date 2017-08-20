using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candy_Collect : MonoBehaviour {

	//[SerializeField] float heightFromFloor = 1;

	// Events
	public delegate void Candy_Collected();
	public static event Candy_Collected OnCandyCollect; // - fire when collected

	ObjInfo info;

	void Awake () {
		info = GetComponent<ObjInfo>();
	}

	void Start () {

		// Snap objects to the ground in some way
//		Ray ray = new Ray (transform.position, -Vector3.up);
//		RaycastHit hit;
//		if (Physics.Raycast (ray, out hit)) {
//			transform.position = hit.point + Vector3.up * heightFromFloor;
//		}

		StartCoroutine(WaitAFrame());
	}

	IEnumerator WaitAFrame()
	{
		yield return null;

		info.LOAD();
	}


	void Update () {
		
	}


	void OnTriggerEnter(Collider col){

		if (col.transform.tag == "Player") {

			if (OnCandyCollect != null)
				OnCandyCollect ();

			info.SAVE(true, false);
			SavingLoading.instance.SaveData();

			Destroy (this.gameObject);

		}
	}

}
