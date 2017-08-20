using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// simple script to make sure the nav objects used for collision detection and movement aren't shown during gameplay

public class NavObjects_TurnOffAtStart : MonoBehaviour {

	void Start () {
		this.transform.gameObject.SetActive (false);
	}

}
