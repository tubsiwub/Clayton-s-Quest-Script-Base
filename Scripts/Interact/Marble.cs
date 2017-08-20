using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System.Reflection;
using System;

[Serializable]
public class Marble : MonoBehaviour
{
	Transform player;
	Renderer rend;
	ObjInfo info;
	Collider col;
	
	[SerializeField] bool floatToPlayer = true;
	[SerializeField] bool collectIfFallen = true;
	[SerializeField] bool collectible = true;
	[SerializeField] bool collected = false;
	[SerializeField] bool shrinking = false;

	const float dist = 0.6f;

	const float defaultCollectDist = 3;
	const float collectDistDontFloat = 1;
	const float deathY = -50;
	float collectDist;

	void Awake()
	{
		info = GetComponent<ObjInfo> ();
	}

	void Start()
	{
		player = GameObject.FindWithTag("Player").transform;
		col = GetComponent<Collider>();

		if (floatToPlayer)
			collectDist = defaultCollectDist;
		else
			collectDist = collectDistDontFloat;

		info.LOAD();
	}

	void Update()
	{
		if (!collected) {
			// If collected by player...!
			if (Vector3.Distance (player.position, transform.position) < collectDist && collectible) {

				// REQUIRED: Play sound!

				collected = true;
				if (col != null) col.enabled = false;

				if (floatToPlayer)
					StartCoroutine(Collected());
				else
					CollectMarble();
			}

			// If falling far below the ground...!
			if (collectIfFallen && transform.position.y < deathY) {

				BeginCollectOffEdge();
			}

			// YOU SPIN ME RIGHT ROUND, BABY!  ROUND, RIGHT ROUND!
			transform.Rotate (0, -1, 0);

		}
	}

	// Called from TreasureChestOpen.cs
	public void DelayCollection(){
		StartCoroutine (DELAY ());
	}
		
	float spinSpeed = 1;
	float zoomSpeed = 1;

	void CollectMarble()
	{
		MarbleManager.instance.GetMarble();
		SaveObject(true, false);
		Destroy(this.gameObject);
	}

	IEnumerator Collected (){

		spinSpeed = 1;
    	zoomSpeed = 1;

		while (true) {

			// Spin around the player
			transform.RotateAround (player.transform.position, Vector3.up, spinSpeed * Time.deltaTime);

			// Keep to the same height as the player
			Vector3 tempPos = transform.position;
			tempPos.y = player.transform.position.y;
			transform.position = Vector3.Lerp (transform.position, tempPos, 4 * Time.deltaTime);

			// Shift slowly toward player as it rotates
			transform.position = Vector3.MoveTowards (transform.position, player.transform.position, zoomSpeed * Time.deltaTime);

			if(Vector3.Distance (player.position, transform.position) < dist && collectible && !shrinking){

				shrinking = true;
				StartCoroutine(Shrinking());

			}

			if (Vector3.Distance (player.position, transform.position) < 0.1f && collectible) {
				CollectMarble();
			}

			spinSpeed += 2.0f;
			zoomSpeed += 0.6f;

			yield return new WaitForEndOfFrame ();
		}

	}

	public void BeginCollectOffEdge()
	{
		// REQUIRED: Play sound!

		collected = true;
		if (col != null) col.enabled = false;
		StartCoroutine(CollectedOffEdge());
	}

	IEnumerator CollectedOffEdge (){

		spinSpeed = 1;
		zoomSpeed = 1;

		bool tooHigh = true;

		while (true) {

			// Spin around the player
			transform.RotateAround (player.transform.position, Vector3.up, spinSpeed * Time.deltaTime);

			// Keep to the same height as the player
			Vector3 tempPos = transform.position;

			if (transform.position.y < player.transform.position.y + 4 && tooHigh) {
				tempPos.y = player.transform.position.y + 4;
			} else {
				tempPos.y = player.transform.position.y;
				tooHigh = false;
			}

			transform.position = Vector3.Lerp (transform.position, tempPos, 4 * Time.deltaTime);

			// Shift slowly toward player as it rotates
			transform.position = Vector3.MoveTowards (transform.position, player.transform.position, zoomSpeed * Time.deltaTime);

			if(Vector3.Distance (player.position, transform.position) < dist && collectible && !shrinking){

				shrinking = true;
				StartCoroutine(Shrinking());

			}

			if (Vector3.Distance (player.position, transform.position) < 0.1f && collectible) {
				CollectMarble();
			}

			spinSpeed += 2.0f;
			zoomSpeed += 3.0f;

			RaycastHit hit = new RaycastHit ();
			if (Physics.Raycast (transform.position, -Vector3.up, out hit, 2)) {

				Vector3 temp = transform.position;
				temp.y = hit.point.y;
				transform.position = temp;

			}

			yield return new WaitForEndOfFrame ();
		}

	}

	IEnumerator Shrinking(){

		while (true) {
			Vector3 tempScale = transform.localScale;
			tempScale *= 0.95f;
			transform.localScale = tempScale;
			zoomSpeed += 2.0f;
			yield return new WaitForEndOfFrame ();
		}

	}

	IEnumerator DELAY(){

		collectible = false;

		yield return new WaitForSeconds (1.0f);

		collectible = true;

		// DELAY is only called for newly spawned marbles
		SaveObject (true, true);
	}

	void SaveObject(bool overwrite, bool exists){

		if (collectible) {

			info.SAVE (overwrite, exists);
			
		} else {

			StartCoroutine (PauseUntilCollectible (overwrite, exists));

		}

		SavingLoading.instance.SaveData();
	}

	IEnumerator PauseUntilCollectible(bool overwrite, bool exists){

		while (!collectible) {

			yield return new WaitForEndOfFrame ();

		}

		info.SAVE (overwrite, exists);

	}
}
