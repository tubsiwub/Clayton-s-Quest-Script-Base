using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupText : MonoBehaviour
{
	[SerializeField] Text drawText;
	RectTransform rectTransform;

	float rectStartX = 0;
	float rectStartY = 0;
	float rectMinY = 235;

	const float popUpSpeed = 20;
	const float popDownSpeed = 20;

	enum PopState { Floor, PoppingUp, PausedUp, PoppingDown }
	PopState popState;
	public bool PoppedDown { get { return popState == PopState.Floor; } }
	public bool PoppedUp { get { return popState == PopState.PausedUp; } }

	bool popUpAgain = false;

	string storedText = "x 0";

	void Awake()
	{
		popState = PopState.Floor;

		drawText = GetComponentInChildren<Text>();
		rectTransform = GetComponent<RectTransform>();

		if (rectStartY == 0)        // make sure this is only set once, so it doesn't get overwriten when at a different position
		{
			rectStartY = rectTransform.anchoredPosition3D.y;
			rectStartX = rectTransform.anchoredPosition3D.x;
		}

		HideText();
		UpdateText();
	}

	public void SetText(string text, bool autoUpdate = true)
	{
		storedText = text;

		if (autoUpdate)
			UpdateText();
	}

	public void DoPopUp()
	{
		StartCoroutine(PopUp(false));
	}

	public void DoPopDown()
	{
		StartCoroutine(PopDown(true));
	}

	public void PopUpPopDown()
	{
		if (popState == PopState.PoppingUp || popState == PopState.PausedUp)
			UpdateText();

		if (popState == PopState.Floor)
			StartCoroutine(PopUp(true));
		else if (!popUpAgain)
			popUpAgain = true;
	}

	public void PopDownPopUp()
	{
		popUpAgain = true;
		StartCoroutine(PopDown(false));
	}

	void UpdateText()
	{
		if (drawText != null)
			drawText.text = storedText;
	}

	public void SetNewX(bool set, int newX)
	{
		if (set)
			rectTransform.anchoredPosition3D = new Vector3(newX, rectTransform.anchoredPosition3D.y);
		else
			rectTransform.anchoredPosition3D = new Vector3(rectStartX, rectTransform.anchoredPosition3D.y);
	}

	public void ShowText()
	{
		StopAllCoroutines();
		popState = PopState.PausedUp;
		rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectStartY, rectTransform.anchoredPosition3D.z);
	}

	public void HideText()
	{
		StopAllCoroutines();
		popState = PopState.Floor;
		rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectMinY, rectTransform.anchoredPosition3D.z);
	}

	IEnumerator PopUp(bool goBackDown)
	{
		popState = PopState.PoppingUp;
		Vector3 target = new Vector3(rectTransform.anchoredPosition3D.x, rectStartY, rectTransform.anchoredPosition3D.z);

		while (Mathf.Abs(rectTransform.anchoredPosition3D.y - rectStartY) > 1)
		{
			target = new Vector3(rectTransform.anchoredPosition3D.x, rectStartY, rectTransform.anchoredPosition3D.z);

			rectTransform.anchoredPosition3D = Vector3.Lerp(rectTransform.anchoredPosition3D, target, Time.deltaTime * popUpSpeed);
			yield return null;
		}

		popState = PopState.PausedUp;
		UpdateText();

		if (goBackDown)
		{
			yield return new WaitForSeconds(1);

			if (popUpAgain)
				StartCoroutine(PopUp(true));
			else
				StartCoroutine(PopDown(true));

			popUpAgain = false;
		}
	}

	IEnumerator PopDown(bool goBackDownOnPopUp)
	{
		popState = PopState.PoppingDown;
		Vector3 target = new Vector3(rectTransform.anchoredPosition3D.x, rectMinY, rectTransform.anchoredPosition3D.z);

		while (Vector3.Distance(rectTransform.anchoredPosition3D, target) > 0.01f)
		{
			target = new Vector3(rectTransform.anchoredPosition3D.x, rectMinY, rectTransform.anchoredPosition3D.z);

			rectTransform.anchoredPosition3D = Vector3.Lerp(rectTransform.anchoredPosition3D, target, Time.deltaTime * popDownSpeed);
			yield return null;
		}

		popState = PopState.Floor;

		if (popUpAgain)
		{
			UpdateText();
			StartCoroutine(PopUp(goBackDownOnPopUp));
		}

		popUpAgain = false;
	}
}
