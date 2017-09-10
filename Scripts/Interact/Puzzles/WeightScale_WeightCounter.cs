using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightScale_WeightCounter : MonoBehaviour {

	public WeightScale_KitchenScale kitScaleScript;

	void Update () 
	{
		this.GetComponent<TextMesh> ().text = (5 - kitScaleScript.weightObjects.Count).ToString();
	}
}
