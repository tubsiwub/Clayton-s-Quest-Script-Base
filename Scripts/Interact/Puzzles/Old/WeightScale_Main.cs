using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Contains data that the weight scales both need to have access to.

public class WeightScale_Main : MonoBehaviour {


	// events
	public delegate void Scale_Balanced();
	public event Scale_Balanced OnScaleBalanced;

	public delegate void Scale_LeftHeavy();
	public event Scale_LeftHeavy OnScaleLeftHeavy;

	public delegate void Scale_RightHeavy();
	public event Scale_RightHeavy OnScaleRightHeavy;



	// Fires the appropriate event based on type
	public void FireEvent(string type){

		if (type.ToLower () == "balanced" || type.ToLower () == "balance") {
			if (OnScaleBalanced != null)
				OnScaleBalanced ();
		}

		if (type.ToLower () == "left" || type.ToLower () == "leftheavy") {
			if (OnScaleLeftHeavy != null)
				OnScaleLeftHeavy ();
		}

		if (type.ToLower () == "right" || type.ToLower () == "rightheavy") {
			if (OnScaleRightHeavy != null)
				OnScaleRightHeavy ();
		}

	}

}
