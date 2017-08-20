using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Origami_Collect : MonoBehaviour
{
	[SerializeField] Vector3 holdOffset;
	[SerializeField] GameObject particleSys;
	[SerializeField] Renderer rend;
	[SerializeField] Material fadeMat;
	[SerializeField] OrigamiManager.OrigamiType origamiType;

	bool collected = false;
	Transform playerHand = null;
	Transform player = null;
	bool held = false;

	const float moveToHandSpeed = 10;

	void Update()
	{
		if (held)
			HandleHold();
	}

	void HandleHold()
	{
		transform.position = GetHoldPos();
	}

	Vector3 GetHoldPos()
	{
		float forward = holdOffset.z;
		float right = holdOffset.x;
		float up = holdOffset.y;
		return playerHand.position + (player.forward * forward) + (player.right * right) + (player.up * up);
	}

	void OnTriggerEnter(Collider col)
	{
		if (collected) return;

		if (col.gameObject.tag == "Player")
		{
			PlayerHandler playerHandler = col.gameObject.GetComponent<PlayerHandler>();

			if (!playerHandler.HasOrigami)
			{
				playerHandler.SetOrigamiAnimation(this);
				collected = true;

				OrigamiManager.instance.ShowUI();
				OrigamiManager.instance.CancelHideRoutine();
			}
		}
	}

	public void Pickup(Transform playerHand, Transform newParent)
	{
		GetComponent<Item_SceneBobble>().enabled = false;
		particleSys.SetActive(false);
		this.playerHand = playerHand;
		player = newParent;

		StartCoroutine(MoveToHand(newParent));
	}

	IEnumerator MoveToHand(Transform newParent)
	{
		Transform startPoint = transform;
		float dist = Vector3.Distance(transform.position, GetHoldPos());
		float duration = dist / moveToHandSpeed;
		float t = 0;

		while (Vector3.Distance(transform.position, GetHoldPos()) > 0.01f)
		{
			t += Time.deltaTime / duration;
			transform.position = Vector3.Lerp(startPoint.position, GetHoldPos(), t);
			transform.localScale = Vector3.Lerp(startPoint.localScale, Vector3.one * 0.5f, t);
			yield return null;
		}

		transform.parent = player;
		transform.rotation = Quaternion.Euler(0, 90, 0) * player.rotation;
		held = true;
	}

	// to do - make a fade effect or something here
	public void Disappear()
	{
		held = false;
		transform.parent = null;
		playerHand = null;
		OrigamiManager.instance.SetCollected(origamiType);

		StartCoroutine(FadeOutAndDie(5));
	}

	IEnumerator FadeOutAndDie(float speed)
	{
		rend.material = fadeMat;

		float a = 1;
		Color col = rend.material.color;
		while (rend.material.color.a > 0.1f)
		{
			a -= speed * Time.deltaTime;

			rend.material.color = new Color(col.r, col.g, col.b, a);
			yield return null;
		}

		rend.material.color = new Color(col.r, col.g, col.b, 0);
		Destroy(gameObject);
	}
}
