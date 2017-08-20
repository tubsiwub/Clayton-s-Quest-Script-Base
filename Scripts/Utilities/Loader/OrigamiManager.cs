using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrigamiManager : MonoBehaviour
{
	public static OrigamiManager instance = null;

	public enum OrigamiType { Crane, Giraffe, Rabbit, Rhino, Turtle };
	Dictionary<OrigamiType, bool> origami;

	RectTransform origamiUI;

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += SceneLoaded;
	}

	void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		if (origamiUI == null)
			origamiUI = GameObject.Find("OrigamiUI").GetComponent<RectTransform>();

		origamiUI.localScale = Vector3.zero;
	}

	void Start()
	{
		origami = new Dictionary<OrigamiType, bool>();

		origami.Add(OrigamiType.Crane, false);
		origami.Add(OrigamiType.Giraffe, false);
		origami.Add(OrigamiType.Rabbit, false);
		origami.Add(OrigamiType.Rhino, false);
		origami.Add(OrigamiType.Turtle, false);

		origamiUI.localScale = Vector3.zero;
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

	public void SetCollected(OrigamiType origamiType)
	{
		origami[origamiType] = true;
		origamiUI.GetComponent<OrigamiUI>().SetCollected(origamiType);

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
