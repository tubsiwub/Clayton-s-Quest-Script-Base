using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadTrigger : MonoBehaviour
{

	// Events
	public delegate void SceneLoadTrigger_Triggered();
	public event SceneLoadTrigger_Triggered OnBallDoorTrigger;

	[SerializeField] bool onlyBall = false;
	[SerializeField] Transform respawnPos;

	string loadName = "";
	public void SetLoadName(string name) { loadName = name; }
	PlayerHandler playerHandler;
	int doneTransitionStamp = 0;

	string WarningText { get { return gameObject.name + " has no scene to load!"; } }

	public enum BuddyState { CreateBuddy, FindBuddy }
	BuddyState buddyState;
	public void SetBuddyState(BuddyState buddyState) { this.buddyState = buddyState; }

	void Start()
	{
		playerHandler = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>();
		ScreenTransition.OnDoneForward += DoneTransition;
	}

	void OnTriggerEnter(Collider col)
	{
		if (onlyBall && playerHandler.CurrentState != PlayerHandler.PlayerState.Ball)
			return;

		if (col.gameObject.tag == "Player")
		{
			if (buddyState == BuddyState.CreateBuddy)
				CreateBuddyAndLoad(respawnPos.transform.position);
			else
				FindBuddyAndLoad();
		}
	}

	void CreateBuddyAndLoad(Vector3 respawnPos)
	{
		playerHandler.SetFrozen(true, true);

		Camera.main.GetComponent<ScreenTransition>().Forward(2, "circle_pattern");
		StartCoroutine("ReallyLoadAfterTransition", respawnPos);
	}

	void DoneTransition()
	{
		doneTransitionStamp = Time.frameCount;
	}

	IEnumerator ReallyLoadAfterTransition(Vector3 respawnPos)
	{
		while (doneTransitionStamp != Time.frameCount)
		{
			yield return null;
		}

		if (loadName == "")
		{
			Debug.LogWarning(WarningText);
			StopCoroutine("ReallyLoadAfterTransition");
			yield return null;
		}

		GameObject obj = new GameObject();
		obj.AddComponent<LoadBuddyDoor>().Init(respawnPos);
		obj.name = "LoadBuddyDoor";
		
		SceneManager.LoadScene(loadName);
	}

	public void FindBuddyAndLoad()
	{
		if (OnBallDoorTrigger != null)
			OnBallDoorTrigger ();

		LoadBuddyDoor loadBuddyDoor = GameObject.Find("LoadBuddyDoor").GetComponent<LoadBuddyDoor>();
		loadBuddyDoor.Load();
	}
}
