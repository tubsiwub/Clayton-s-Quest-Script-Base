using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFade : MonoBehaviour
{
	[SerializeField] Text[] texts;

	void Start()
	{
		StartCoroutine(FadeAwayTexts());
	}

	void Update()
	{
		
	}

	IEnumerator FadeAwayTexts()
	{
		yield return new WaitForSeconds(5);

		while (texts[0].color.a > 0.1f)
		{
			for (int i = 0; i < texts.Length; i++)
				texts[i].color = new Color(1, 1, 1, texts[i].color.a - Time.deltaTime);

			yield return null;
		}

		for (int i = 0; i < texts.Length; i++)
			texts[i].color = new Color(1, 1, 1, 0);
	}
}
