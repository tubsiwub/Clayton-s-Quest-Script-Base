using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sitable : MonoBehaviour
{
	[SerializeField] Renderer[] glowRends;

	Color[] startCols;
	PlayerHandler playerHandler;
	bool canSit = false;
	bool canStandUp = false;

	void Start()
	{
		playerHandler = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>();
		startCols = new Color[glowRends.Length];

		for (int i = 0; i < glowRends.Length; i++)
			startCols[i] = glowRends[i].material.color;
	}

	void Update()
	{
		if (!canSit) return;

		if (Input.GetButtonDown("Interact") && !playerHandler.IsFrozen)
		{
			playerHandler.SetSit(true, transform.forward);
			playerHandler.SetFrozen(true, false);
			playerHandler.SetDetectCollisions(false);
			SetGlow(false);

			StartCoroutine(StandUpTimer());
		}

		if ((Input.GetButtonDown("Interact") ||
			Input.GetButtonDown(PlayerHandler.JumpString) ||
			Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.2f ||
			Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.2f)
			&& playerHandler.IsFrozen && canStandUp)
		{
			playerHandler.SetSit(false, transform.forward);
			playerHandler.SetDetectCollisions(true);
			SetGlow(true);
			canStandUp = false;
			// wait for the animation event to turn off frozen
		}
	}

	IEnumerator StandUpTimer()
	{
		yield return new WaitForSeconds(0.7f);
		canStandUp = true;
	}

	void SetGlow(bool set)
	{
		if (set && glowRends[0].material.color == Color.green) return;
		if (!set && glowRends[0].material.color != Color.green) return;
		if (set && playerHandler.IsFrozen) return;

		for (int i = 0; i < glowRends.Length; i++)
			glowRends[i].material.color = set ? Color.green : startCols[i];
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag != "Player") return;

		canSit = playerHandler.CanSit;
		playerHandler.SeatInRange = canSit && !canStandUp && !playerHandler.IsFrozen;
		SetGlow(canSit);
	}

	void OnTriggerStay(Collider col)
	{
		if (col.gameObject.tag != "Player") return;

		canSit = playerHandler.CanSit;
		playerHandler.SeatInRange = canSit && !canStandUp && !playerHandler.IsFrozen;
		SetGlow(canSit);
	}

	void OnTriggerExit(Collider col)
	{
		if (col.gameObject.tag != "Player") return;

		canSit = false;
		playerHandler.SeatInRange = false;
		SetGlow(false);
	}
}
