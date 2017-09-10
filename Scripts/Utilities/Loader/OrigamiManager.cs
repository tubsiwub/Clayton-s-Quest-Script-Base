using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrigamiManager : MonoBehaviour
{
	public static OrigamiManager instance = null;

	public enum OrigamiType { Crane, Giraffe, Rabbit, Rhino, Turtle };
	Dictionary<OrigamiType, bool> origami;

	RectTransform origamiUI;

	public bool CollectedAll { get {

		foreach (KeyValuePair<OrigamiType, bool> entry in origami)
		{
			if (entry.Value == false)
				return false;
		}

			return true;
	} }

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);
		origami = SetupOrigamiDictionary();

		SceneManager.sceneLoaded += SceneLoaded;
	}

	void Start()
	{
		origamiUI.localScale = Vector3.zero;
	}

	void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		if (origamiUI == null)
			origamiUI = GameObject.Find("OrigamiUI").GetComponent<RectTransform>();

		origamiUI.localScale = Vector3.zero;

		// only load when not on start scene
		if (scene.buildIndex != 0)
			SavingLoading.instance.LoadOrigami();

		InitOrigamiUI();
	}

	public static Dictionary<OrigamiType, bool> SetupOrigamiDictionary()
	{
		Dictionary<OrigamiType, bool> origami = new Dictionary<OrigamiType, bool>();

		origami.Add(OrigamiType.Crane, false);
		origami.Add(OrigamiType.Giraffe, false);
		origami.Add(OrigamiType.Rabbit, false);
		origami.Add(OrigamiType.Rhino, false);
		origami.Add(OrigamiType.Turtle, false);

		return origami;
	}

	public void ShowUI()
	{
		origamiUI.localScale = Vector3.one;
	}

	public void HideUI()
	{
		origamiUI.localScale = Vector3.zero;
	}

	IEnumerator WaitThenHide(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		HideUI();
	}

	public void CancelHideRoutine()
	{
		StopCoroutine("WaitThenHide");
	}

	void InitOrigamiUI()
	{
		foreach (KeyValuePair<OrigamiType, bool> entry in origami)
		{
			if (entry.Value == true)
				origamiUI.GetComponent<OrigamiUI>().SetCollected(entry.Key);
		}
	}

	public void SetOrigamiDirect(Dictionary<OrigamiType, bool> origami)
	{
		List<OrigamiType> entriesToChange = new List<OrigamiType>();

		foreach (KeyValuePair<OrigamiType, bool> entry in this.origami)
		{
			if (entry.Value == false && origami[entry.Key] == true)
				entriesToChange.Add(entry.Key);
		}

		// cannot change dictionary while iterating over it, so mark what to change, then do it after
		for (int i = 0; i < entriesToChange.Count; i++)
			SetCollected(entriesToChange[i]);
	}

	public void SetCollected(OrigamiType origamiType)
	{
		origami[origamiType] = true;
		origamiUI.GetComponent<OrigamiUI>().SetCollected(origamiType);

		SavingLoading.instance.SaveOrigami(origami);
		SavingLoading.instance.SaveData();

		CancelHideRoutine();
		StartCoroutine("WaitThenHide", 3);
		StartCoroutine(WaitForPause(origamiType));
	}

	IEnumerator WaitForPause(OrigamiType origamiType)
	{
		while (Time.timeScale != 0)
		{
			yield return null;
		}

		yield return null;
		GameObject.Find("OrigamiMenuUI").GetComponent<OrigamiUI>().SetCollected(origamiType);
	}
}
