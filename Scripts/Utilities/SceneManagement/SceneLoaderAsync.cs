using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoaderAsync : MonoBehaviour
{
	[SerializeField] string sceneName;
	[SerializeField] Slider loadBar;

	void Start()
	{
		StartCoroutine(LoadTheScene());
	}

	IEnumerator LoadTheScene()
	{
		AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);

		while (!ao.isDone)
		{
			loadBar.value = ao.progress;
			yield return null;
		}
	}
}
