using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthPie : MonoBehaviour
{
	[SerializeField] Image[] healthBars;
	const float fadeSpeed = 5;

	public void ResetBars()
	{
		StopAllCoroutines();

		for (int i = 0; i < healthBars.Length; i++)
		{
			healthBars[i].color = new Color(1, 1, 1, 1);
		}
	}

	public void LoseABar(int health)
	{
		StopAllCoroutines();

		int barToFade = Mathf.Abs(health - 4);

		if (healthBars[barToFade].color.a == 1)
			StartCoroutine(FadeOutBar(barToFade));
	}

	public void LoseAllBars()
	{
		StopAllCoroutines();

		for (int i = 0; i < healthBars.Length; i++)
		{
			StartCoroutine(FadeOutBar(i));
		}
	}

	public void RegainBars(int lives)
	{
		StopAllCoroutines();

		int converted = Mathf.Abs(lives - 4);
		for (int i = healthBars.Length-1; i >= converted; i--)
		{
			if (healthBars[i].color.a != 1)
				StartCoroutine(FadeInBar(i));
		}
	}

	IEnumerator FadeOutBar(int ndx)
	{
		float a = 1;
		while (healthBars[ndx].color.a > 0.1f)
		{
			a -= Time.deltaTime * fadeSpeed;
			healthBars[ndx].color = new Color(1, 1, 1, a);
			yield return null;
		}
		healthBars[ndx].color = new Color(1, 1, 1, 0);
	}

	IEnumerator FadeInBar(int ndx)
	{
		float a = 0;
		while (healthBars[ndx].color.a < 0.9f)
		{
			a += Time.deltaTime * fadeSpeed;
			healthBars[ndx].color = new Color(1, 1, 1, a);
			yield return null;
		}
		healthBars[ndx].color = new Color(1, 1, 1, 1);
	}
}
