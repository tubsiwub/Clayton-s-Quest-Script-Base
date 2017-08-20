using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorkCoverMug : MonoBehaviour {

	public PuzzleButton buttonObj;

	Animator anim;

	void Start () {
		buttonObj.OnButtonActivated += CoverMug;
		anim = GetComponent<Animator> ();
	}

	void CoverMug(){
		anim.SetTrigger ("START");
	}
}
