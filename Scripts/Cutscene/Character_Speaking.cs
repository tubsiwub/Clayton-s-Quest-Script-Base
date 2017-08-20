using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Speaking : MonoBehaviour {

	Animator animator;

	void Start(){
		INITIALIZE ();
	}

	public void INITIALIZE(){

		animator = GetComponent<Animator> ();

		animator.Play ("Character_Talk");

	}
}
