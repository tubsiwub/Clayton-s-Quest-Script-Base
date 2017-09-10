using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class IconControllerSwapper : MonoBehaviour
{
	[SerializeField] GameObject kbIcon;
	[SerializeField] GameObject controllerIcon;

	bool hasController = false;

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

		kbIcon.SetActive(!hasController);
		controllerIcon.SetActive(hasController);
	}

	void GetControllerState()
	{
		GamePadState gamePadState = GamePad.GetState(0);
		hasController = gamePadState.IsConnected;
	}
}
