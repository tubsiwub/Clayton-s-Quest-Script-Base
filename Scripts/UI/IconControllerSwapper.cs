using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class IconControllerSwapper : MonoBehaviour
{
	[SerializeField] GameObject kbIcon;
	[SerializeField] GameObject controllerIcon;

	bool hasController = false;
	bool lastHasCont = true;

	void Start()
	{
		DoSwitchCheck();
	}

	void Update()
	{
		if (Time.frameCount % 120 == 0)
			DoSwitchCheck();
	}

	void DoSwitchCheck()
	{
		GetControllerState();

		if (hasController != lastHasCont)
		{
			kbIcon.SetActive(!hasController);
			controllerIcon.SetActive(hasController);
		}

		lastHasCont = hasController;
	}

	void GetControllerState()
	{
		GamePadState gamePadState = GamePad.GetState(0);
		hasController = gamePadState.IsConnected;
	}
}
