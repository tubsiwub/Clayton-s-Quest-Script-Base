using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
	[SerializeField] Transform respawnPos;
	public Transform RespawnPos { get { return respawnPos; } }
	public bool Activated { get { return playerHandler.LastCheckpoint == this; } }

	protected PlayerHandler playerHandler;
	protected Animator anim;

	void Awake()
	{
		name = name + "_" + MathFunctions.GetStringFromPosition(transform.position);
		anim = GetComponentInChildren<Animator>();
	}

	protected void Start()
	{
		playerHandler = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>();
	}

	public virtual void Activate()
	{
		anim.SetBool("FlagUp", true);
	}

	public virtual void Deactivate()
	{
		anim.SetBool("FlagUp", false);
	}

	protected virtual void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Player" && playerHandler.Ready)
			playerHandler.SetCheckpoint(this);
	}
}
