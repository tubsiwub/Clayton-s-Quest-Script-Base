using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class ButtonHintText : MonoBehaviour
{
	PopupText popupText;
	PlayerHandler playerHandler;
	PlayerHolder playerHolder;

	const string pickupTextKB = "Press E to Pickup";
	const string pickupTextCont = "Press Y to Pickup";

	const string throwTextKB = "Left Click to Throw";
	const string throwTextCont = "Press X to Throw";

	const string talkTextKB = "Press E to Talk";
	const string talkTextCont = "Press Y to Talk";

	const string sitTextKB = "Press E to have a seat";
	const string sitTextCont = "Press Y to have a seat";

	string PickupText { get { return hasController ? pickupTextCont : pickupTextKB; } }
	string ThrowText { get { return hasController ? throwTextCont : throwTextKB; } }
	string TalkText { get { return hasController ? talkTextCont : talkTextKB; } }
	string SitText { get { return hasController ? sitTextCont : sitTextKB; } }

	void Start()
	{
		popupText = GetComponent<PopupText>();
		playerHandler = GameObject.FindWithTag("Player").GetComponentInChildren<PlayerHandler>();
		playerHolder = GameObject.FindWithTag("Player").GetComponentInChildren<PlayerHolder>();

		GetControllerState();
	}

	bool hasController = false;
	bool poppedThrowText = false;
	bool showingNpcText = false;

	public void ShowNPCText()
	{
		showingNpcText = true;
		popupText.StopAllCoroutines();

		if (popupText.PoppedUp)
		{
			popupText.SetText(TalkText, false);
			popupText.PopDownPopUp();
		}
		else
		{
			popupText.SetText(TalkText);
			popupText.DoPopUp();
		}
	}

	public void CancelNPCText()
	{
		if (!showingNpcText)
			return;

		popupText.StopAllCoroutines();
		popupText.DoPopDown();
		showingNpcText = false;
	}

	void Update()
	{
		if (Time.frameCount % 120 == 0)
			GetControllerState();

		if (showingNpcText) return;

		if (playerHolder.IsHolding)
			HandleThrowText();
		else
		{
			bool isBall = playerHandler.CurrentState == PlayerHandler.PlayerState.Ball;

			HandlePickupText(isBall);
			HandleSitText();
			HandleHideText(isBall);
		}
	}

	void GetControllerState()
	{
		GamePadState gamePadState = GamePad.GetState(0);
		hasController = gamePadState.IsConnected;
	}

	void HandleThrowText()
	{
		if (!poppedThrowText)
		{
			popupText.SetText(ThrowText, false);

			popupText.StopAllCoroutines();
			popupText.PopDownPopUp();
			poppedThrowText = true;
		}
	}

	void HandlePickupText(bool isBall)
	{
		if (poppedThrowText && popupText.PoppedUp)
		{
			popupText.SetText(PickupText, false);

			popupText.StopAllCoroutines();
			popupText.DoPopDown();
			poppedThrowText = false;
		}

		if (playerHolder.HasObjectsNearby && popupText.PoppedDown && !isBall)
		{
			popupText.SetText(PickupText);

			popupText.StopAllCoroutines();
			popupText.DoPopUp();
		}
	}

	void HandleSitText()
	{
		if (playerHandler.SeatInRange && popupText.PoppedDown)
		{
			popupText.SetText(SitText);

			popupText.StopAllCoroutines();
			popupText.DoPopUp();
		}
	}

	void HandleHideText(bool isBall)
	{
		if (((!playerHolder.HasObjectsNearby && !playerHandler.SeatInRange) || isBall) && popupText.PoppedUp)
		{
			popupText.StopAllCoroutines();
			popupText.DoPopDown();
		}
	}
}
