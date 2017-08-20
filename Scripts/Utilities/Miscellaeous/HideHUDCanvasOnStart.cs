using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HideHUDCanvasOnStart : MonoBehaviour
{
	Canvas canvas;

	void Awake()
	{
		canvas = GetComponent<Canvas>();
		SceneManager.sceneLoaded += SceneLoaded;
	}

	void SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == LevelManager.START_SCENE)
			canvas.enabled = false;
	}
}
