using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
	[SerializeField] Animator humanAnimator;
	[SerializeField] GameObject rightHand;
	[SerializeField] GameObject leftHand;

	public GameObject RightHand { get { return rightHand; } }
	public GameObject LeftHand { get { return leftHand; } }

	PlayerHandler playerHandler;

	bool attacking = false;
	bool lastAttacking = false;

	public enum AttackState { PreAttack, Attack1, Attack2, Attack3 };
	AttackState attackState;
	public bool IsAttacking { get { return attackState != AttackState.PreAttack; } }

	float hitboxStartSize;
	const float MaxHitboxSize = 2.75f;
	const float HitBoxGrowSpeed = 6.00f;

	int disableStamp = 0;
	bool RecentlyDisabled { get { return Time.frameCount - disableStamp <= 10; } }
	
	void Awake()
	{
		attackState = AttackState.PreAttack;
		rightHand.SetActive(false);
		leftHand.SetActive(false);

		hitboxStartSize = rightHand.transform.localScale.x;
		playerHandler = GetComponent<PlayerHandler>();
	}

	void OnDisable()
	{
		Disable();
	}

	void Disable()
	{
		if (RecentlyDisabled) return;
		
		disableStamp = Time.frameCount;
		if (humanAnimator.isInitialized)
		{
			humanAnimator.SetBool("Attacking", false);
			humanAnimator.CrossFade("Attack Idle", 0.1f);
		}

		ResetHitboxSizes();
		SetAttackState(AttackState.PreAttack);
		lastAttacking = false;
	}

	void Update()
	{
		if (!CanAttack()) { Disable(); return; }

		if (Input.GetButtonDown(PlayerHandler.AttackString))
			attacking = true;
		
		if (attacking != lastAttacking)
			humanAnimator.SetBool("Attacking", attacking);

		lastAttacking = attacking;

		HealthManager.instance.SetExternalIsInvincible
			(rightHand.activeInHierarchy || leftHand.activeInHierarchy);
	}

	bool CanAttack()
	{
		if (playerHandler.IsFrozen) return false;
		if (playerHandler.AboutToFallDie) return false;
		if (!PlayerHandler.CanUpdate) return false;
		if (HealthManager.instance.IsPaused) return false;

		return true;
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

				{
					object[] info = new object[] { leftHand, true };
					StartCoroutine("GrowHitbox", info);
				}

				SoundManager.instance.PlayClip("AttackChain1" + (Random.Range(0, 2) == 0 ? "A" : "B"));
			break;
			
			case AttackState.Attack2:
				rightHand.SetActive(true);
				leftHand.SetActive(false);

				{
					object[] info = new object[] { rightHand, true };
					StartCoroutine("GrowHitbox", info);
				}

				SoundManager.instance.PlayClip("AttackChain2" + (Random.Range(0, 2) == 0 ? "A" : "B"));
			break;
			
			case AttackState.Attack3:
				rightHand.SetActive(true);
				leftHand.SetActive(true);

				{
					object[] info1 = new object[] { rightHand, false };
					object[] info2 = new object[] { leftHand, false };

					StartCoroutine("GrowHitbox", info1);
					StartCoroutine("GrowHitbox", info2);
				}

				SoundManager.instance.PlayClip("AttackChain3A");// + (Random.Range(0, 2) == 0 ? "A" : "B"));
			break;
		}
	}

	IEnumerator GrowHitbox(object[] info)
	{
		GameObject hitbox = (GameObject)info[0];
		bool turnOff = (bool)info[1];

		float scale = hitboxStartSize;
		while (hitbox.transform.localScale.x < MaxHitboxSize)
		{
			scale += Time.deltaTime * HitBoxGrowSpeed;
			hitbox.transform.localScale = new Vector3(scale, scale, scale);
			yield return null;
		}

		if (turnOff)
			hitbox.SetActive(false);
	}

	void ResetHitboxSizes()
	{
		StopCoroutine("GrowHitbox");
		
		Vector3 scale = new Vector3(hitboxStartSize, hitboxStartSize, hitboxStartSize);
		rightHand.transform.localScale = scale;
		leftHand.transform.localScale = scale;
	}
}
