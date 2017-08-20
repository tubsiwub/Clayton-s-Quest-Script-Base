using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BallDoorControl : MonoBehaviour
{
	[SerializeField] Collider colliderToToggle;
	[SerializeField] string loadPath = "";
	public enum DoorType { Enter, Exit }
	[SerializeField] DoorType doorType;

	Animator anim;
	Transform player;
	bool opened = false;
	const float distToOpen = 2;

	void Awake()
	{
		// do this in Awake before SceneLoadTrigger has time to spit out warning
		SceneLoadTrigger sceneLoad = transform.Find("SceneLoadTrigger").GetComponent<SceneLoadTrigger>();
		sceneLoad.SetLoadName(PathToName(loadPath));
		sceneLoad.SetBuddyState(doorType == DoorType.Enter ? SceneLoadTrigger.BuddyState.CreateBuddy : SceneLoadTrigger.BuddyState.FindBuddy);
	}

	void Start()
	{
		if (colliderToToggle == null)
			Debug.LogWarning(gameObject.name + " is missing a collider to toggle!");

		anim = GetComponentInChildren<Animator>();
		player = GameObject.FindWithTag("Player").transform;
	}

	string PathToName(string path)
	{
		string name = "";

		for (int i = path.Length-1; i >= 0; i--)
		{
			if (path[i] == '/')
				break;

			name += path[i];
		}

		if (name != "")
		{
			name = name.Remove(0, 6);
			name = Reverse(name);
		}

		return name;
	}

	string Reverse(string s)
	{
		char[] charArray = s.ToCharArray();
		Array.Reverse(charArray);
		return new string(charArray);
	}

	void Update()
	{
		bool nearDoor = Vector3.Distance(transform.position, player.position) < distToOpen;

		if (!opened && nearDoor)
		{
			anim.SetTrigger("Open");
			opened = true;
		}

		if (opened && !nearDoor)
		{
			anim.SetTrigger("Close");
			opened = false;
		}
	}

	void OnTriggerStay(Collider col)
	{
		if (colliderToToggle == null) return;

		if (col.gameObject.tag == "Player")
			colliderToToggle.enabled = false;
	}

	void OnTriggerExit(Collider col)
	{
		if (colliderToToggle == null) return;

		if (col.gameObject.tag == "Player")
			colliderToToggle.enabled = true;
	}
}
