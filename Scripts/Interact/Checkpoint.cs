using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
	[SerializeField] Transform respawnPos;
	[SerializeField] GameObject fireworks;
	[SerializeField] bool showFireworks = true;

	public Transform RespawnPos { get { return respawnPos; } }
	public bool Activated { get { return playerHandler.LastCheckpoint == this; } }

	protected PlayerHandler playerHandler;
	protected Animator anim;

	Vector3 fireworksOffset = new Vector3(0, 3, 0);
	GameObject fireworksRef = null;
	const float FireworksStayTime = 3;
	const float FireworksSpawnTime = 0.40f;

	void Awake()
	{
		name = "Checkpoint_" + MathFunctions.GetHexStringFromPosition(transform.position);
		anim = GetComponentInChildren<Animator>();
	}

	void Start()
	{
		playerHandler = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>();
	}

	IEnumerator WaitThenFireworks(float t)
	{
		yield return new WaitForSeconds(t);
		SpawnFireworks();
	}

	void SpawnFireworks()
	{
		Vector3 point = transform.Find("Flag").position + fireworksOffset;

		fireworksRef = Instantiate(fireworks, point, Quaternion.identity);
		StartCoroutine(KillFireworks(FireworksStayTime));
	}

	public void Activate()
	{
		anim.SetBool("AnimateInstant", false);
		anim.SetBool("FlagUp", true);

		if (fireworksRef == null && showFireworks)
			StartCoroutine(WaitThenFireworks(FireworksSpawnTime));
	}

	public void ActivateInstant()
	{
		anim.SetBool("AnimateInstant", true);
		anim.SetBool("FlagUp", true);
	}

	public void Deactivate()
	{
		anim.SetBool("AnimateInstant", false);
		anim.SetBool("FlagUp", false);
	}

	protected void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Player" && playerHandler.Ready)
			playerHandler.SetCheckpoint(this);
	}

	IEnumerator KillFireworks(float t)
	{
		yield return new WaitForSeconds(t);
		Destroy(fireworksRef);
		fireworksRef = null;
	}
}
