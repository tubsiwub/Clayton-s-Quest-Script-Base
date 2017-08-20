using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMarbleTextPos : MonoBehaviour
{
	[SerializeField] PopupText marbleText;
	PopupText candyText;
	bool lastPopped = false;

	void Start()
	{
		candyText = GetComponent<PopupText>();
	}

	void Update()
	{
		if (lastPopped != candyText.PoppedUp)
			marbleText.SetNewX(candyText.PoppedUp, 950);

		lastPopped = candyText.PoppedUp;
	}
}
