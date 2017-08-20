using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetGame : MonoBehaviour {

	void Update () {
		if (Input.GetKeyDown (KeyCode.F7)) {
			GetComponent<SceneLoadTrigger> ().FindBuddyAndLoad ();
		}
	}
}
