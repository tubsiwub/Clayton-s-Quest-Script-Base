using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshMaterial_RainbowHueShift : MonoBehaviour {

	[Range(0.01f, 0.1f)]
	public float transitionSpeed;

	[Range(0.01f, 1.0f)]
	public float transparency;

	Color color;

	float r, g, b;

	void Start () {

		color = new Color (1.0f, 1.0f, 1.0f, transparency);

		r = g = b = 1.0f;

		StartCoroutine (HueShift ("red"));

	}

	IEnumerator HueShift(string colorFocus){

		switch (colorFocus) {

		case "red":

			while (color.r > 0.1f) {

				r -= transitionSpeed;

				if (g < 1.0f)
					g += 0.1f;

				if (b < 1.0f)
					b += 0.1f;

				color = new Color (r, g, b, transparency);

				yield return new WaitForEndOfFrame ();
			}

			r = 0;
			g = 1.0f; 
			b = 1.0f;
			color = new Color (r, g, b, transparency);

			StartCoroutine (HueShift ("green"));

			break;

		case "green":

			while (color.g > 0.1f) {

				g -= transitionSpeed;

				if (r < 1.0f)
					r += 0.1f;

				if (b < 1.0f)
					b += 0.1f;

				color = new Color (r, g, b, transparency);

				yield return new WaitForEndOfFrame ();
			}

			g = 0;
			r = 1.0f; 
			b = 1.0f;
			color = new Color (r, g, b, transparency);

			StartCoroutine (HueShift ("blue"));

			break;

		case "blue":

			while (color.b > 0.1f) {

				b -= transitionSpeed;

				if (r < 1.0f)
					r += 0.1f;

				if (g < 1.0f)
					g += 0.1f;

				color = new Color (r, g, b, transparency);

				yield return new WaitForEndOfFrame ();
			}

			b = 0;
			g = 1.0f; 
			r = 1.0f;
			color = new Color (r, g, b, transparency);

			StartCoroutine (HueShift ("red"));

			break;

		}

	}


	void Update () {

		GetComponent<Renderer> ().material.color = color;

	}
}
