using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;

public class AnalyticsManager : MonoBehaviour
{
	public static AnalyticsManager instance = null;

	List<SceneEventPos> triggerList;

	class SceneEventPos
	{
		public SceneEventPos(string sceneName, string eventName, Vector3 position)
		{
			this.sceneName = sceneName;
			this.eventName = eventName;
			this.position = position;
		}

		public string sceneName;
		public string eventName;
		public Vector3 position;
	}

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);

		InitTriggerList();
		SceneManager.sceneLoaded += SceneLoaded;
	}

	void InitTriggerList()
	{
		triggerList = new List<SceneEventPos>();

		triggerList.Add(new SceneEventPos("Tutorial Zone", "tutorialStep1", new Vector3(57, 7.5f, -212)));
		triggerList.Add(new SceneEventPos("Tutorial Zone", "tutorialStep2", new Vector3(195, 8, -56)));
		triggerList.Add(new SceneEventPos("Tutorial Zone", "tutorialStep3", new Vector3(346, 16, -114)));

		triggerList.Add(new SceneEventPos("Pickup Zone", "pickupStep1", new Vector3(713, 1.6f, 161)));
		triggerList.Add(new SceneEventPos("Pickup Zone", "pickupStep2", new Vector3(697, 23, 305)));
		triggerList.Add(new SceneEventPos("Pickup Zone", "pickupStep3", new Vector3(878, 21, 480)));

		triggerList.Add(new SceneEventPos("Mountain Zone", "mountainStep1", new Vector3(684, 0.3f, -341)));
		triggerList.Add(new SceneEventPos("Mountain Zone", "mountainStep2", new Vector3(740.5f, 16.54f, -395)));
		triggerList.Add(new SceneEventPos("Mountain Zone", "mountainStep3", new Vector3(759.2f, 100.98f, -628.5f)));
	}

	void SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		StartCoroutine(WaitABitThenSpawn(scene));
	}

	IEnumerator WaitABitThenSpawn(Scene scene)
	{
		yield return new WaitForSeconds(0.5f);

		for (int i = 0; i < triggerList.Count; i++)
		{
			if (triggerList[i].sceneName == scene.name)
			{
				CreateTrigger(triggerList[i]);
			}
		}
	}

	void CreateTrigger(SceneEventPos sceneEventPos)
	{
		string objName = "EventTrigger_" + sceneEventPos.eventName;

		GameObject trigger = new GameObject();
		trigger.AddComponent<AnalyticsEventTrigger>();
		trigger.GetComponent<AnalyticsEventTrigger>().Init(sceneEventPos.eventName, 15);
		trigger.transform.position = sceneEventPos.position;
		trigger.name = objName;
	}

	void OnDestroy()
	{
		Analytics.FlushEvents();
	}
}
