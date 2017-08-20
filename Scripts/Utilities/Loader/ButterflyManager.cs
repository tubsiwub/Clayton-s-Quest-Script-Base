using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButterflyManager : MonoBehaviour
{
	public static ButterflyManager instance = null;

	[SerializeField] GameObject butterfly;
	[SerializeField] bool allGolden = false;

	Transform player;
	Vector3 playerStartPoint;
	Transform cam;

	const float walkDistNeeded = 50;
	const float spawnDistFromCam = 30;
	const float maxHeightDifFromPlayer = 20;

	const float minSpawnHeightFromGround = 2;
	const float maxSpawnHeightFromGround = 4;
	const float maxTargetPosOffset = 2;
	const int maxSpawnAtATime = 3;
	const int chanceOfGolden = 25;

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += (scene, loadingMode) => { SceneLoaded(); };

		GetRefs();
	}

	void SceneLoaded()
	{
		GetRefs();
	}

	void GetRefs()
	{
		if (player == null)
			player = GameObject.FindWithTag("Player").transform;

		if (cam == null)
			cam = Camera.main.transform;

		if (player != null)
			playerStartPoint = player.position;
	}

	void Update()
	{
		if (player == null) return;

		if (Vector3.Distance(player.position, playerStartPoint) > walkDistNeeded)
		{
			playerStartPoint = player.position;
			int amount = 0;

			if (Random.Range(0, 10) == 0) amount = maxSpawnAtATime;	// very rarely, spawn the MAX
			else amount = (int)Mathf.Floor(Random.Range(1, maxSpawnAtATime)); // otherwise, random 1 below max

			for (int i = 0; i < amount; i++)
				SpawnButterfly();
		}
	}

	bool SpawnButterfly()
	{
		Vector3 target = cam.position + (cam.forward * spawnDistFromCam);
		target += new Vector3(Random.Range(-maxTargetPosOffset, maxTargetPosOffset), 0, 
			Random.Range(-maxTargetPosOffset, maxTargetPosOffset));
		target.y = 300;

		RaycastHit hitInfo;
		Ray ray = new Ray(target, Vector3.down);
		if (Physics.Raycast(ray, out hitInfo, 600, -1, QueryTriggerInteraction.Ignore))
		{
			float heightDiff = Mathf.Abs(hitInfo.point.y - player.position.y);
			
			if (heightDiff > maxHeightDifFromPlayer)
				return false;

			target.y = hitInfo.point.y + Random.Range(minSpawnHeightFromGround, maxSpawnHeightFromGround);

			bool isGolden = Random.Range(0, allGolden ? 0 : chanceOfGolden) == 0;
			Instantiate(butterfly, target, Quaternion.identity).GetComponent<ButterflyFloat>().Init(isGolden);
			return true;
		}

		return false;
	}
}
