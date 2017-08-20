using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadTrigger : MonoBehaviour
{
	[SerializeField] bool onlyBall = false;
	[SerializeField] Transform respawnPos;

	string loadName = "";
	public void SetLoadName(string name) { loadName = name; }
	PlayerHandler playerHandler;

	string WarningText { get { return gameObject.name + " has no scene to load!"; } }

	public enum BuddyState { CreateBuddy, FindBuddy }
	BuddyState buddyState;
	public void SetBuddyState(BuddyState buddyState) { this.buddyState = buddyState; }

	void Start()
	{
		playerHandler = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>();
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
		if (loadName == "")
		{
			Debug.LogWarning(WarningText);
			return;
		}

		GameObject obj = new GameObject();
		obj.AddComponent<LoadBuddyDoor>().Init(respawnPos);
		obj.name = "LoadBuddyDoor";

		// when loading into a ball door from here:
		// cancel the stored player position, so the player starts at the start point
		LevelManager.instance.CancelPlayerData();
		SceneManager.LoadScene(loadName);
	}

	public void FindBuddyAndLoad()
	{
		LoadBuddyDoor loadBuddyDoor = GameObject.Find("LoadBuddyDoor").GetComponent<LoadBuddyDoor>();
		loadBuddyDoor.Load();
	}
}
