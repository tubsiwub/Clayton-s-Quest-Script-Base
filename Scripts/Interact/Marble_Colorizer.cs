using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marble_Colorizer : MonoBehaviour {

	List<Color> acceptedColors;

	void Start () {

		FillList ();

		int randomColor = Random.Range (0, acceptedColors.Count);

		if (this.gameObject.name != "innerMarble")
		GetComponent<Renderer> ().material.color = acceptedColors[randomColor];

	}

	void FillList(){

		acceptedColors = new List<Color> ();

		acceptedColors.Add (new Color (1.0f, 0.0f, 0.0f, 0.1607f));		// PURE RED
		acceptedColors.Add (new Color (1.0f, 0.0f, 0.725f, 0.1607f));	// MAGENTA
		acceptedColors.Add (new Color (0.447f, 0.0f, 1.0f, 0.1607f));	// DEEP PURPLE
		acceptedColors.Add (new Color (0.0f, 0.1725f, 1.0f, 0.1607f));	// DUSK BLUE
		acceptedColors.Add (new Color (0.0f, 1.0f, 1.0f, 0.1607f));		// PURE TEAL
		acceptedColors.Add (new Color (0.0f, 1.0f, 0.09f, 0.1607f));	// NEON GREEN
		acceptedColors.Add (new Color (1.0f, 0.89f, 0.0f, 0.1607f));	// GOLD

	}
}
