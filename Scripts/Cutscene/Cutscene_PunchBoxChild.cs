using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscene_PunchBoxChild : MonoBehaviour {

	GameObject rightHand, leftHand;
	BallController ballControl;

	void Start () {

		rightHand = GameObject.FindWithTag ("Player").GetComponent<PlayerAttack> ().RightHand;
		leftHand = GameObject.FindWithTag ("Player").GetComponent<PlayerAttack> ().LeftHand;
		ballControl = GameObject.FindWithTag ("Player").GetComponent<BallController> ();

	}

	void OnTriggerEnter(Collider col) {

		if (col.gameObject == rightHand || col.gameObject == leftHand || (col.tag == "Player" && ballControl.IsSlamming)) {

			transform.parent.GetComponent<Cutscene_PunchBox> ().FireTrigger (this.gameObject);

		}
	}
}
