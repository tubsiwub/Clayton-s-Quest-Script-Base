using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MarbleManager : MonoBehaviour
{
	public static MarbleManager instance = null;

	PopupText popupText;

	int collected = 0;
	public int Collected { get { return collected; } }

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += SceneLoaded;

		SavingLoading.instance.LoadMarbles();
	}

	void SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (GameObject.Find("MarbleUI"))
		{
			popupText = GameObject.Find("MarbleUI").GetComponent<PopupText>();

			popupText.HideText();
			popupText.SetText("x " + collected);
		}
	}

	public void SetMarbles(int amount)
	{
		collected = amount;
	}

	public void GetMarble()
	{
		collected++;
		SavingLoading.instance.SaveMarbles(collected);

		popupText.SetText("x " + collected, false);
		popupText.PopUpPopDown();
	}

	public void RemoveMarble()
	{
		collected--;
		if (collected < 0)
			collected = 0;

		SavingLoading.instance.SaveMarbles(collected);

		popupText.SetText("x " + collected, false);
		popupText.PopUpPopDown();
	}

	public void ShowText()
	{
		popupText.ShowText();
	}

	public void HideText()
	{
		popupText.HideText();
	}
}
