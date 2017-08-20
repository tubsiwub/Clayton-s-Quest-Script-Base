using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuItems
{
	[MenuItem("Tools/Create Missing Assets")]
	static void CreateMissingAssets()
	{
		if (!GameObject.FindWithTag("Player"))
		{
			if (Camera.main != null)
			{
				Undo.DestroyObjectImmediate(Camera.main.gameObject);
				Debug.Log("Destroyed original camera");
			}

			CreatePrefab("Player", true);
		}

		if (!GameObject.Find("Loader"))
			CreatePrefab("Loader", true);

		if (!GameObject.Find("MainHUDCanvas"))
			CreatePrefab("MainHUDCanvas", false);

		if (!GameObject.Find("DialogueCanvas"))
			CreatePrefab("DialogueCanvas", false);

		if (!GameObject.Find("LetterboxCanvas"))
			CreatePrefab("LetterboxCanvas", false);
		
		if (!GameObject.Find("EventSystem"))
		{
			GameObject eventSys = new GameObject();
			eventSys.AddComponent<EventSystem>();
			eventSys.AddComponent<StandaloneInputModule>();
			eventSys.name = "EventSystem";

			Undo.RegisterCreatedObjectUndo(eventSys, "Create " + eventSys.name);
			Debug.Log("Created " + eventSys.name);
		}
	}

	static void CreatePrefab(string name, bool setPosition)
	{
		GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(Resources.Load(name));

		if (setPosition)
			obj.transform.position = Vector3.zero;

		Undo.RegisterCreatedObjectUndo(obj, "Create " + name);
		Debug.Log("Created " + name);
	}

	static void CreateObject(string name, bool setPosition)
	{
		GameObject obj = (GameObject)Object.Instantiate(Resources.Load("EventSystem"));
		obj.name = name;

		if (setPosition)
			obj.transform.position = Vector3.zero;

		Undo.RegisterCreatedObjectUndo(obj, "Create " + name);
		Debug.Log("Created " + name);
	}
}
