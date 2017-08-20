// hi, I'm used to let people load their active scene from SceneBase automatically

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadBuddy : MonoBehaviour
{
	string[] scenesToLoad;
	public string[] ScenesToLoad { get { return scenesToLoad; } }

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	public void Init(string sceneToLoad)
	{
		scenesToLoad = new string[1];
		scenesToLoad[0] = sceneToLoad;
	}

	public void Init(string[] scenesToLoad)
	{
		this.scenesToLoad = scenesToLoad;
	}

	public void Init(List<string> sceneList)
	{
		scenesToLoad = new string[sceneList.Count];

		for (int i = 0; i < sceneList.Count; i++)
			scenesToLoad[i] = sceneList[i];
	}
}
