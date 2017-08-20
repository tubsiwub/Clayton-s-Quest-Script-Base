using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DistanceCuller : MonoBehaviour
{
	public static DistanceCuller instance = null;

	List<GameObject> objects;
	Transform player;

	float distFromPlayer = 60;
	const int refreshRate = 40;

	public void SetDistFromPlayer(float dist) { distFromPlayer = dist; }

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += (scene, loadingMode) => { SceneLoaded(); };
		objects = new List<GameObject>();
		// we don't need to Init() on SceneUnloaded, since Cull() removed null objects anyway
	}

	void SceneLoaded()
	{
		Init();
	}

	void Init()
	{
		SetAllActive();     // do this before we clear list, so we can find all active tagged objects
		objects.Clear();

		player = GameObject.FindWithTag("Player").transform;

		AddTagToList("Marble");
		AddTagToList("Grass");

		Cull();
	}

	void AddTagToList(string tagName)
	{
		GameObject[] objs = GameObject.FindGameObjectsWithTag(tagName);

		for (int i = 0; i < objs.Length; i++)
		{
			objects.Add(objs[i]);
			objs[i].SetActive(false);
		}
	}

	void SetAllActive()
	{
		for (int i = 0; i < objects.Count; i++)
		{
			if (objects[i] != null)
				objects[i].SetActive(true);
		}
	}

	// may not need to be used, since Cull() now does this each time it runs
	public void RemoveObject(GameObject obj)
	{
		for (int i = 0; i < objects.Count; i++)
		{
			if (obj == objects[i])
			{
				objects.Remove(objects[i]);
				return;
			}
		}
	}

	void Update()
	{
		if (Time.frameCount % refreshRate == 0)
			Cull();
	}

	void Cull()
	{
		for (int i = objects.Count-1; i >= 0; i--)
		{
			if (objects[i] == null)
			{
				objects.RemoveAt(i);
				continue;
			}

			bool playerNear = Vector3.Distance(player.position, objects[i].transform.position) <
				distFromPlayer;

			objects[i].SetActive(playerNear);
		}
	}
}
