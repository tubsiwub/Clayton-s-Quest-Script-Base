using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
	[SerializeField] Animator humanAnimator;
	[SerializeField] GameObject rightHand;
	[SerializeField] GameObject leftHand;

	// Public accessors
	public GameObject RightHand { get { return rightHand; } }
	public GameObject LeftHand { get { return leftHand; } }

	//FistHitbox rightHitbox;
	//FistHitbox leftHitbox;

	PlayerHandler playerHandler;
	//HumanController humanController;

	bool attacking = false;
	bool lastAttacking = false;

	public enum AttackState { PreAttack, Attack1, Attack2, Attack3 };
	AttackState attackState;
	public bool IsAttacking { get { return attackState != AttackState.PreAttack; } }

	float hitboxStartSize;
	const float maxHitboxSize = 175;
	const float hitboxGrowSpeed = 280;
	
	void Start()
	{
		attackState = AttackState.PreAttack;
		rightHand.SetActive(false);
		leftHand.SetActive(false);

		hitboxStartSize = rightHand.transform.localScale.x;

		//rightHitbox = rightHand.GetComponent<FistHitbox>();
		//leftHitbox = leftHand.GetComponent<FistHitbox>();
		playerHandler = GetComponent<PlayerHandler>();
		//humanController = GetComponent<HumanController>();
	}

	void OnDisable()
	{
		if (humanAnimator.isInitialized)
		{
			humanAnimator.SetBool("Attacking", false);
			humanAnimator.CrossFade("Attack Idle", 0.1f);
		}

		ResetHitboxSizes();
	}

	void Update()
	{
		if (playerHandler.IsFrozen) return;
		if (!PlayerHandler.CanUpdate) return;

		if (Input.GetButtonDown(PlayerHandler.AttackString))
			attacking = true;
		
		if (attacking != lastAttacking)
			humanAnimator.SetBool("Attacking", attacking);

		lastAttacking = attacking;

		HealthManager.instance.SetExternalIsInvincible
			(rightHand.activeInHierarchy || leftHand.activeInHierarchy);
	}

	public void SetAttackState(AttackState newState)		// called from animation states
	{
		attackState = newState;

		attacking = false;
		ResetHitboxSizes();

		switch (attackState)
		{
			case AttackState.PreAttack:
				rightHand.SetActive(false);
				leftHand.SetActive(false);
				playerHandler.SetFaceAnimation("Smile");

			break;
			
			case AttackState.Attack1:
				rightHand.SetActive(false);
				leftHand.SetActive(true);
				// only set face animation in 1 and 3 (carries from 1 to 2)
				playerHandler.SetFaceAnimation("Angry");
				StartCoroutine("GrowHitbox", leftHand);

				SoundManager.instance.PlayClip("AttackChain1" + (Random.Range(0, 2) == 0 ? "A" : "B"));
			break;
			
			case AttackState.Attack2:
				rightHand.SetActive(true);
				leftHand.SetActive(false);
				StartCoroutine("GrowHitbox", rightHand);

				SoundManager.instance.PlayClip("AttackChain2" + (Random.Range(0, 2) == 0 ? "A" : "B"));
			break;
			
			case AttackState.Attack3:
				rightHand.SetActive(true);
				leftHand.SetActive(true);
				StartCoroutine("GrowHitbox", rightHand);
				StartCoroutine("GrowHitbox", leftHand);

				SoundManager.instance.PlayClip("AttackChain3" + (Random.Range(0, 2) == 0 ? "A" : "B"));
			break;
		}
	}

	IEnumerator GrowHitbox(GameObject hitbox)
	{
		float scale = hitboxStartSize;
		while (hitbox.transform.localScale.x < maxHitboxSize)
		{
			scale += Time.deltaTime * hitboxGrowSpeed;
			hitbox.transform.localScale = new Vector3(scale, scale, scale);
			yield return null;
		}
	}

	void ResetHitboxSizes()
	{
		StopCoroutine("GrowHitbox");

		Vector3 scale = new Vector3(hitboxStartSize, hitboxStartSize, hitboxStartSize);
		rightHand.transform.localScale = scale;
		leftHand.transform.localScale = scale;
	}
}
