using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Purpose:  Renames all children of object with unique names in an ordered fashion 
//	ensuring the same names are assigned every time.


// THIS CLASS IS DEAD, THERE IS NO NEED

public class RenameChildren : MonoBehaviour {

	public string childName;

	void Awake () {

		/*
		for (int i = 0; i < this.transform.childCount; i++)
			this.transform.GetChild (i).name = childName + i.ToString ();
		*/
	}

}
