using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrigamiUI : MonoBehaviour
{
	[SerializeField] OrigamiIcon[] icons;

	public void SetCollected(OrigamiManager.OrigamiType origamiType)
	{
		for (int i = 0; i < icons.Length; i++)
		{
			if (icons[i].OrigamiType == origamiType)
				icons[i].SetCollected();
		}
	}
}
