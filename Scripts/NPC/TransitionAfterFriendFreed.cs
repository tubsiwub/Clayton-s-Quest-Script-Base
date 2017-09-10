using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionAfterFriendFreed : MonoBehaviour
{
	int transitionDoneStamp = 0;

	void Start()
	{
		ScreenTransition.OnDoneForward += DoneScreenTransition;
	}

	void DoneScreenTransition()
	{
		transitionDoneStamp = Time.frameCount;
	}

	public void DoneFriendFreed()
	{
		// DO NOT transition while we're in main scenes, only ball doors
		if (LevelManager.instance.SceneBaseLoaded)
		{
			GameObject.FindWithTag("Player").GetComponent<PlayerHandler>().SetFrozen(false, false);
			return;
		}

		Camera.main.GetComponent<ScreenTransition>().Forward(2, "circle_pattern");
		StartCoroutine("WaitForScreenTransition");
	}

	IEnumerator WaitForScreenTransition()
	{
		while (transitionDoneStamp != Time.frameCount)
		{
			yield return null;
		}

		FindBuddyAndLoad();
	}

	void FindBuddyAndLoad()
	{
		LoadBuddyDoor loadBuddyDoor = GameObject.Find("LoadBuddyDoor").GetComponent<LoadBuddyDoor>();
		loadBuddyDoor.Load();
	}
}
