using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPlatformScript : MonoBehaviour
{

	bool isFalling = false;
	bool playerDetected = false;
	bool platFell = false;

	public float force;
	Rigidbody rb;

	public GameObject player;
	public GameObject card;

	//float distance;

	public float fallTimerStart;
	float fallTimer;
	float resetTimer = 5.0f;

	Vector3 startPos;
	Quaternion startRot;

	public Renderer rend;
//	float colorTimer = 0.5f;

	Color color;
//	Color altColor = Color.red;

	Animator anim;

	public float alphaChange;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.FindWithTag("Player");

		anim = card.GetComponent<Animator>();

		rb = GetComponent<Rigidbody>();
		rb.isKinematic = true;

		startPos = transform.position;
		startRot = transform.rotation;

		//color = rend.material.color;

//		rend = GetComponent<Renderer>();
//		rend.enabled = true;
//		rend.material.color = color;
	}
	
	// Update is called once per frame
	void Update () 
	{
		//Debug.Log (fallTimer);
		float elapsed = Time.deltaTime;

		if (playerDetected)
		{
			fallTimer -= elapsed;
			//colorTimer -= elapsed;
			anim.SetBool ("Wobble", true);
		}

		else
		{
			fallTimer = fallTimerStart;
			//colorTimer = 0.5f;
			anim.SetBool ("Wobble", false);
		}

		if (isFalling)
		{
			rb.isKinematic = false;
			transform.Translate (Vector3.down * Time.deltaTime * force, Space.World);
		}

		//distance = Vector3.Distance (transform.position, player.transform.position);

		if (fallTimer <= 0) 
		{
			isFalling = true;
			platFell = true;
			anim.SetBool ("Wobble", false);

			//color.a -= alphaChange;
		}

		else
		{
			isFalling = false;
		}

		//Color change stuff
//		if (colorTimer == 0.5)
//			rend.material.color = color;
//		else if (colorTimer <= 0.45f && colorTimer > 0.4f)
//			rend.material.color = altColor;
//		else if (colorTimer <= 0.4f && colorTimer > 0.3f)
//			rend.material.color = color;
//		else if (colorTimer <= 0.3f && colorTimer > 0.2f)
//			rend.material.color = altColor;
//		else  if(colorTimer <= 0.2f && colorTimer > 0.1f)
//			rend.material.color = color;
//		else if (colorTimer <= 0)
//			colorTimer = 0.5f;
			

		//Debug.Log(colorTimer);

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
			playerDetected = false;

			StartCoroutine(FadeTo(1.0f, 2.0f));
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			playerDetected = true;
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
