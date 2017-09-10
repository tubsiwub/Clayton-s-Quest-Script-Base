using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CageExplode : MonoBehaviour
{
	//event 
	public delegate void Cage_Explode();
	public event Cage_Explode OnCageExplode;

	[SerializeField] string npcName;

	[SerializeField] GameObject startCage;
	[SerializeField] GameObject brokenCage;
	[SerializeField] Rigidbody cageCrown;
	[SerializeField] Collider punchCollider;

	[SerializeField] GameObject[] disableObjects;
	[SerializeField] Rigidbody[] pieces;
	Vector3[] startPos;
	Quaternion[] startRot;
	Transform player;

	const float pushForce = 10;
	const float piecesKinematicTime = 1.25f;
	const float piecesVanishTime = 3;
	bool canAttack = true;

	string storageKey = "";

	void Start () {

		// Event Triggers
		if (GetComponent<SavingLoading_StorageKeyCheck> ()) {
			GetComponent<SavingLoading_StorageKeyCheck> ().OnKeyCheck += KeyCheck;
			storageKey = GetComponent<SavingLoading_StorageKeyCheck> ().storageKey;
		}

		startPos = new Vector3[pieces.Length];
		startRot = new Quaternion[pieces.Length];
		player = GameObject.FindWithTag("Player").transform;

		for (int i = 0; i < pieces.Length; i++)
		{
			startPos[i] = pieces[i].transform.position;
			startRot[i] = pieces[i].transform.rotation;
		}
	}

	/*void Update()
	{
		if (Input.GetKeyDown(KeyCode.O) && !canAttack)
		{
			ResetCage();
		}
	}*/

	// If storageKey marks this as completed, perform these actions
	void KeyCheck(){

		GetComponent<SavingLoading_StorageKeyCheck> ().OnKeyCheck -= KeyCheck;

		GetComponent<SavingLoading_StorageKeyCheck> ().enabled = false;

		Destroy (this.gameObject);
	}

	public void BreakCage()
	{
		if (!canAttack) return;

		// event
		if (OnCageExplode != null)
			OnCageExplode ();

		for (int i = 0; i < disableObjects.Length; i++)
			disableObjects[i].SetActive(false);

		punchCollider.enabled = false;
		startCage.SetActive(false);
		brokenCage.SetActive(true);

		Vector3 direction = (transform.position - player.position).normalized;
		for (int i = 0; i < pieces.Length; i++)
		{
			pieces[i].AddForce(direction * pushForce, ForceMode.VelocityChange);
		}

		// Save NPC name to list of NPCs - order doesn't matter
		SavingLoading.instance.SaveNPC (npcName);
		GetComponent<SavingLoading_StorageKeyCheck> ().OnKeyCheck -= KeyCheck;
		GetComponent<SavingLoading_StorageKeyCheck> ().enabled = false;
		SavingLoading.instance.SaveStorageKey (storageKey, true);

		// Store NPC into list for town - temporary for now
		//NPC_Manager.instance.StoreNPC("Charles" + Time.frameCount);

		StartCoroutine("VanishPieces");
		canAttack = false;
	}

	IEnumerator VanishPieces()
	{
		yield return new WaitForSeconds(piecesKinematicTime);

		for (int i = 0; i < pieces.Length; i++)
		{
			if (pieces[i] != cageCrown)
			{
				pieces[i].isKinematic = true;
				pieces[i].GetComponent<Collider>().enabled = false;
			}
		}

		yield return new WaitForSeconds(piecesVanishTime);
		brokenCage.SetActive(false);
	}

	void ResetCage()
	{
		StopCoroutine("VanishPieces");
		brokenCage.SetActive(false);

		for (int i = 0; i < disableObjects.Length; i++)
			disableObjects[i].SetActive(true);

		punchCollider.enabled = true;

		for (int i = 0; i < pieces.Length; i++)
		{
			pieces[i].isKinematic = false;
			pieces[i].GetComponent<Collider>().enabled = true;
			pieces[i].transform.position = startPos[i];
			pieces[i].transform.rotation = startRot[i];
		}
		startCage.SetActive(true);
		canAttack = true;
	}
}
