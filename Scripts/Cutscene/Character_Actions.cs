using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Purpose:

// Placement:  Character cutscene component - scene object for cutscene

public class Character_Actions : MonoBehaviour {

	public string actionType;

	public Transform PointA;
	public Transform PointB;

	public float speed = 4.0f;

	Animator animator;

	void Start(){
		INITIALIZE ();
	}

	public void INITIALIZE () {

		animator = GetComponent<Animator> ();

		switch (actionType.ToLower()) {

		case "moving":

			animator.Play ("Character_Walk");
			break;

		case "talking":

			animator.Play ("Character_Talk");
			break;

		}


	}



	void Update () {

		switch (actionType.ToLower()) {

		case "moving":

			transform.LookAt (PointB);
			transform.Translate (Vector3.forward * Time.deltaTime * speed);
			break;

		case "talking":

			// N/A
			break;

		}

	}



	void OnTriggerEnter(Collider col){

		switch (actionType.ToLower()) {

		case "moving":

			transform.position = PointA.position;

			break;

		case "talking":

			// N/A
			break;

		}
			
	}




}
