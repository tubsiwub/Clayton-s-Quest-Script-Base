using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tristan_TreasureChestOpen : MonoBehaviour {

	public string prefabLocation;

	Animator anim;

	public int rewardsAmount = 3;
	public GameObject typeReward;
	List<GameObject> spawnedRewards;

	GameObject MarbleParent;

	ObjInfo info;

	void Awake()
	{
		info = GetComponent<ObjInfo> ();
	}

	void Start () {

		if (GameObject.Find ("Marbles")) {
			MarbleParent = GameObject.Find ("Marbles");
		}
		else {
			MarbleParent = new GameObject ();
			MarbleParent.name = "Marbles";
		}

		anim = GetComponent<Animator> ();

		spawnedRewards = new List<GameObject> ();

		SceneLoaded ();
	}

	void SceneLoaded(){

		info.LOAD();

	}

	void Update () {

		GameObject player = GameObject.FindWithTag ("Player");

		float distance = Vector3.Distance (player.transform.position, this.transform.position);

		if (distance < 2.4f) {

			StartCoroutine (SpawnRewards ());

			StartCoroutine (DestroyChest ());

		}

	}

	void SaveObject(bool overwrite, bool exists){


		info.SAVE (overwrite, exists);
		SavingLoading.instance.SaveData();
	}

	IEnumerator SpawnRewards(){

		anim.SetTrigger ("Open");

		GetComponent<BoxCollider> ().enabled = false;
		GetComponent<Rigidbody> ().useGravity = false;
		GetComponent<Rigidbody> ().isKinematic = true;

		yield return new WaitForSeconds (1.0f);	// time of animation, above

		// Spawn marbles!
		for (int i = 0; i < rewardsAmount; i++) {
			float randomX = transform.position.x + i;
			float randomZ = transform.position.z + i;
			spawnedRewards.Add ((GameObject)Instantiate (typeReward, new Vector3(randomX, transform.position.y + 2, randomZ), Quaternion.Euler (new Vector3 (0, 0, 0))));
		}

		foreach (GameObject obj in spawnedRewards) {

			// Fire the marble cannons!  Boom boom!
			obj.GetComponent<Rigidbody> ().velocity += new Vector3(
				Random.Range(-1,1),
				Random.Range(4,7),
				Random.Range(-1,1));

			transform.SetParent (MarbleParent.transform);

			obj.GetComponent<Marble> ().DelayCollection ();

			// Generate a random number to create a unique name with
			int randomNameNumberical = (int)Mathf.Ceil(Random.Range (0, 9999999999));

			// If you can find a marble with this exact name, try again
			while (GameObject.Find ("SpawnedRewardMarble" + randomNameNumberical)) {
				randomNameNumberical = (int)Mathf.Ceil(Random.Range (0, 9999999999));
			}

			// Set marble name for saving/loading purposes
			obj.transform.name = "SpawnedRewardMarble" + randomNameNumberical;
		}

	}

	IEnumerator DestroyChest (){

		yield return new WaitForSeconds (1.0f);

		SaveObject(true, false);
		Destroy(transform.gameObject);

	}
}
