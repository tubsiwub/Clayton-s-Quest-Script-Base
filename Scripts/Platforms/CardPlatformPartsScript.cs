using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPlatformPartsScript : MonoBehaviour 
{
	public GameObject trigger;
	public GameObject player;
	public GameObject card;

	Rigidbody rb;

	public float fallTimerStart;
	float fallTimer;
	float resetTimer = 5.0f;
	public float force;

	bool isFalling = false;
	bool platFell = false;

	Vector3 startPos;
	Quaternion startRot;

	public Renderer rend;
	Color color;

	Animator anim;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.FindWithTag("Player");

		anim = card.GetComponent<Animator>();

		rb = GetComponent<Rigidbody>();
		rb.isKinematic = true;

		startPos = transform.position;
		startRot = transform.rotation;
	}

	// Update is called once per frame
	void Update () 
	{
		float elapsed = Time.deltaTime;

		if (trigger.GetComponent<CardPlatformTriggerScript>().platTriggered == true)
		{
			fallTimer -= elapsed;

			anim.SetBool ("Wobble", true);
		}

		else
		{
			fallTimer = fallTimerStart;
			anim.SetBool ("Wobble", false);
		}

		if (isFalling)
		{
			rb.isKinematic = false;
			transform.Translate (Vector3.down * Time.deltaTime * force, Space.World);
		}


		if (fallTimer <= 0) 
		{
			isFalling = true;
			platFell = true;
			anim.SetBool ("Wobble", false);
		}

		else
		{
			isFalling = false;
		}

		if (platFell == true)
		{
			resetTimer -= elapsed;
			StartCoroutine(FadeTo(0.0f, 1.0f));
		}

		if (resetTimer <= 0)
		{
			transform.position = startPos;
			transform.rotation = startRot;
			resetTimer = 5.0f;
			rb.isKinematic = true;
			platFell = false;
			trigger.GetComponent<CardPlatformTriggerScript>().platTriggered = false;

			StartCoroutine(FadeTo(1.0f, 2.0f));
		}
	}

	IEnumerator FadeTo(float value, float time)
	{
		float alpha = rend.material.color.a;

		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / time)
		{
			Color newColor = new Color (rend.material.color.r, rend.material.color.g, rend.material.color.b, Mathf.Lerp (alpha, value, t));
			rend.material.color = newColor;

			yield return null;
		}
	}
}