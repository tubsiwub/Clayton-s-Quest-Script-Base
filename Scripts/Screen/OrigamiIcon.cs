using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrigamiIcon : MonoBehaviour
{
	[SerializeField] Sprite foundSprite;
	[SerializeField] OrigamiManager.OrigamiType origamiType;
	public OrigamiManager.OrigamiType OrigamiType { get { return origamiType; } }

	public void SetCollected()
	{
		GetComponent<Image>().sprite = foundSprite;
	}
}
