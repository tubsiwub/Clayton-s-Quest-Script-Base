using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockScript : MonoBehaviour 
{

	public float torque;
	public float force;

	public GameObject player;

	public Renderer rend;
	public bool despawn = false;

	float alpha;

	Vector3 startPos;
	Quaternion startRot;

	Vector3 startVel;

	Rigidbody rb;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.FindWithTag("Player");
		rb = GetComponent<Rigidbody>();

		startPos = transform.position;
		startRot = transform.rotation;

		startVel = rb.velocity;
	}
	
	// Update is called once per frame
	void Update () 
	{
		rb.AddTorque((transform.right * -1) * torque);

		if (despawn == true)
		{
			transform.position = startPos;
			transform.rotation = startRot;
			rb.velocity = startVel;

			despawn = false;

			//StartCoroutine(FadeTo(1.0f, 0.5f));
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			Vector3 pushDir = player.transform.position - transform.position;
			pushDir.y = 0;
			pushDir.Normalize();

			HealthManager.instance.LoseALifeAndPushAway(pushDir, force);
		}

		if (other.tag == "BlockDespawner")
		{
			despawn = true;
		}
	}

	IEnumerator FadeTo(float value, float time)
	{
		alpha = rend.material.color.a;

		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / time)
		{
			Color newColor = new Color (rend.material.color.r, rend.material.color.g, rend.material.color.b, Mathf.Lerp (alpha, value, t));
			rend.material.color = newColor;

			yield return null;
		}
	}
}
