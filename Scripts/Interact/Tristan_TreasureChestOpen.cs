using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tristan_TreasureChestOpen : MonoBehaviour {

	Animator anim;

	public int rewardsAmount = 3;
	public GameObject typeReward;
	//List<GameObject> spawnedRewards;

	bool opened = false;

	GameObject playerObj;

	ObjInfo info;

	void Awake()
	{
		info = GetComponent<ObjInfo> ();
	}

	void Start () {

		anim = GetComponent<Animator> ();
		playerObj = GameObject.FindWithTag ("Player");

		//spawnedRewards = new List<GameObject> ();

		SceneLoaded ();
	}

	void SceneLoaded(){

		info.LOAD();

	}

	void Update () {

		GameObject player = GameObject.FindWithTag ("Player");

		float distance = Vector3.Distance (player.transform.position, this.transform.position);

		if (distance < 2.4f && !opened) {

			//StartCoroutine (SpawnRewards ());
	
			anim.SetTrigger ("Open");
	
			StartCoroutine (ChestRewardDelay (0.75f));

			opened = true;
		}

	}

	IEnumerator ChestRewardDelay(float waitTime)
	{
		yield return new WaitForSeconds (waitTime);

		GetComponent<BoxCollider> ().enabled = false;
		GetComponent<Rigidbody> ().useGravity = false;
		GetComponent<Rigidbody> ().isKinematic = true;

		for (int i = 0; i < rewardsAmount; i++) {
			StartCoroutine (SpawnReward (i));
		}

		StartCoroutine (DestroyChest ());
	}

	void SaveObject(bool overwrite, bool exists){


		info.SAVE (overwrite, exists);
		SavingLoading.instance.SaveData();
	}

	IEnumerator SpawnReward(float i){

		yield return new WaitForSeconds (i / 100.0f);

		// spawn a marble
		GameObject reward; 
		reward = (GameObject)Instantiate (typeReward, playerObj.transform.position + Random.insideUnitSphere * 12, Quaternion.identity);
		if (reward.GetComponent<Collider> ())
			reward.GetComponent<Collider> ().enabled = false;
		StartCoroutine (FlyTowardPlayer (reward));
	}

	IEnumerator FlyTowardPlayer(GameObject reward){

		while (reward) {
			reward.transform.position = Vector3.MoveTowards (reward.transform.position, playerObj.transform.position, 20 * Time.deltaTime);
			yield return new WaitForEndOfFrame ();
		}

	}

//	IEnumerator SpawnRewards(){
//
//		anim.SetTrigger ("Open");
//
//		GetComponent<BoxCollider> ().enabled = false;
//		GetComponent<Rigidbody> ().useGravity = false;
//		GetComponent<Rigidbody> ().isKinematic = true;
//
//		yield return new WaitForSeconds (1.0f);	// time of animation, above
//
//		// Spawn marbles!
//		for (int i = 0; i < rewardsAmount; i++) {
//			float randomX = transform.position.x + i;
//			float randomZ = transform.position.z + i;
//			spawnedRewards.Add ((GameObject)Instantiate (typeReward, new Vector3(randomX, transform.position.y + 2, randomZ), Quaternion.Euler (new Vector3 (0, 0, 0))));
//		}
//
//		foreach (GameObject obj in spawnedRewards) {
//
//			// Fire the marble cannons!  Boom boom!
//			obj.GetComponent<Rigidbody> ().velocity += new Vector3(
//				Random.Range(-1,1),
//				Random.Range(4,7),
//				Random.Range(-1,1));
//
//			obj.GetComponent<Marble> ().DelayCollection ();
//		}
//
//	}

	IEnumerator DestroyChest (){

		yield return new WaitForSeconds (1.0f);

		SaveObject(true, false);
		Destroy(transform.gameObject);

	}
}
