using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
	public GameObject[] block;
	GameObject spawnedBlock;

	public float spawnDelay;

	// Use this for initialization
	void Start ()
	{
		StartCoroutine (DelayedSpawn(spawnDelay));
	}

	// Update is called once per frame
	void Update ()
	{

	}

	IEnumerator DelayedSpawn(float delay)
	{
		yield return new WaitForSeconds(delay);

		int rand = Random.Range (0, 4);

		if (rand == 0)
			spawnedBlock = block [0];

		if (rand == 1)
			spawnedBlock = block [1];

		if (rand == 2)
			spawnedBlock = block [2];

		if (rand == 3)
			spawnedBlock = block [3];
		
		Instantiate (spawnedBlock, transform.position, transform.rotation);
	}
}
