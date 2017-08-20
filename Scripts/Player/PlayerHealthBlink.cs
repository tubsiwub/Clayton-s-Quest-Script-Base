using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthBlink : MonoBehaviour
{
	[SerializeField] Renderer[] rends;

	Color[] startColors;
	const int blinkAmount = 10;
	const float timeBetweenBlinks = 0.15f;
	PlayerHandler playerHandler;

	void Start()
	{
		startColors = new Color[rends.Length];
		for (int i = 0; i < startColors.Length; i++)
		{
			startColors[i] = rends[i].material.color;
		}

		playerHandler = GetComponent<PlayerHandler>();
	}

	public void StartBlinking(bool setFace)
	{
		if (setFace) playerHandler.SetFaceAnimation("Distraught", 0.6f);
		StartCoroutine("DoBlink");
	}

	public void StopBlinking()
	{
		StopCoroutine("DoBlink");
	}

	public void SetTint(bool doTint, bool blinkColor)
	{
		for (int i = 0; i < startColors.Length; i++)
		{
			if (doTint)
				rends[i].material.color = blinkColor ? Color.red : new Color(0.2f, 0.2f, 0.2f);
			else
				rends[i].material.color = startColors[i];
		}
	}

	IEnumerator DoBlink()
	{
		for (int i = 0; i < blinkAmount; i++)
		{
			if (i % 2 == 0)
				SetTint(true, true);
			else
				SetTint(false, true);

			yield return new WaitForSeconds(timeBetweenBlinks);
		}

		SetTint(false, true);
		HealthManager.instance.DoneBlinking();
	}
}
