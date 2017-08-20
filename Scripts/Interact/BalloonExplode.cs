using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonExplode : MonoBehaviour
{

	bool balloonActive = true;	//when false, balloon isn't interactable
	public bool BalloonActive { get { return balloonActive; } }

	[SerializeField] GameObject startBalloon;
	[SerializeField] GameObject brokenBalloon;
	[SerializeField] Collider punchCollider;

	[SerializeField] GameObject[] disableObjects;
	[SerializeField] Rigidbody[] pieces;
	Vector3[] startPos;
	Quaternion[] startRot;
	//Transform player;

	const float pushForce = 10;
	const float piecesVanishTime = 4;
	bool canAttack = true;

	Cutscene_PunchBox punchBox;

	void Start()
	{
		if(GetComponent<Cutscene_PunchBox> ())
			punchBox = GetComponent<Cutscene_PunchBox> ();
		if(GetComponent<Cutscene_PunchBoxChild> ())
			punchBox = transform.parent.GetComponent<Cutscene_PunchBox> ();

		startPos = new Vector3[pieces.Length];
		startRot = new Quaternion[pieces.Length];
		//player = GameObject.FindWithTag("Player").transform;

		for (int i = 0; i < pieces.Length; i++)
		{
			startPos[i] = pieces[i].transform.position;
			startRot[i] = pieces[i].transform.rotation;
		}
	}

	public void BreakBalloon()
	{
		if (!canAttack) return;

		StartCoroutine (SetInactive ());

		for (int i = 0; i < disableObjects.Length; i++)
			disableObjects[i].SetActive(false);

		punchCollider.enabled = false;
		startBalloon.SetActive(false);
		brokenBalloon.SetActive(true);

		//Vector3 direction = (transform.position - player.position).normalized;
		for (int i = 0; i < pieces.Length; i++)
		{
			//pieces[i].AddForce(direction * pushForce, ForceMode.VelocityChange);
			pieces[i].AddExplosionForce(10, transform.position, 3, 0, ForceMode.VelocityChange);
		}

		StartCoroutine("VanishPieces");
		canAttack = false;
	}

	IEnumerator SetInactive(){

		yield return new WaitForSeconds (1);

		balloonActive = false;	

	}

	IEnumerator VanishPieces()
	{
		yield return new WaitForSeconds(piecesVanishTime);
		brokenBalloon.SetActive(false);

		while (punchBox.GetCooldown > 0)
			yield return new WaitForEndOfFrame ();

		StartCoroutine ("Respawn");
	}

	IEnumerator Respawn()
	{
		yield return new WaitForSeconds (5);
		ResetBalloon ();
	}

	void ResetBalloon()
	{
		balloonActive = true;

		StopCoroutine("VanishPieces");
		brokenBalloon.SetActive(false);

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
		startBalloon.SetActive(true);
		canAttack = true;
	}
}
