using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ruler_BreakTilt : MonoBehaviour {

	Animator anim;

	void Start(){
		anim = GetComponent<Animator> ();
	}

	// When player touches ruler, it breaks
	void OnTriggerEnter(Collider col){
		if (col.transform.tag == "Player") {
			anim.SetTrigger ("BreakTilt");
		}
	}

	// Reset ruler to default state before break-bend
	public void Reset(){
		anim.ResetTrigger ("BreakTilt");
		anim.SetTrigger ("Reset");
	}
}
