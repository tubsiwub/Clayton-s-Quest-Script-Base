// Tristan

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue_TextBorders : MonoBehaviour {

	public GameObject dialogueBG;
	 
	void Awake () {

		this.GetComponent<RectTransform> ().position = dialogueBG.GetComponent<RectTransform>().position;
		this.GetComponent<RectTransform> ().offsetMax = new Vector2 (436, -8.1f);	// -right, -top
		this.GetComponent<RectTransform> ().offsetMin = new Vector2 (-338.1f,-135.4f);	// left, bottom
	}

	void Update () {



	}
}
