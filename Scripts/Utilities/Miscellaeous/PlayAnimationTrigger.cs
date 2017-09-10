using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnimationTrigger : MonoBehaviour
{
	[SerializeField] string triggerName;
	[SerializeField] bool lockScale = false;
	[SerializeField] bool allowPunching = true;

	Animator anim;
	Vector3 startScale;
	bool canTrigger = true;

	int lastPunchStamp = 0;
	const int PunchTimeout = 10;
	bool CanPunch { get { return Time.frameCount - lastPunchStamp > PunchTimeout; } }

	void Start()
	{
		anim = GetComponent<Animator>();
		startScale = transform.localScale;
	}

	void Update()
	{
		if (lockScale)
			transform.localScale = startScale;
	}

	void LateUpdate()
	{
		if (lockScale)
			transform.localScale = startScale;
	}

	void OnTriggerEnter(Collider col)
	{
		if ((col.gameObject.tag == "Player" && canTrigger))
			SetTrigger();

		if (allowPunching && col.gameObject.GetComponent<FistHitbox>() && CanPunch)
			SetTrigger();
	}

	void SetTrigger()
	{
		anim.SetTrigger(triggerName);
		canTrigger = false;
		lastPunchStamp = Time.frameCount;
	}

	void OnTriggerExit(Collider col)
	{
		if (col.gameObject.tag == "Player")
			canTrigger = true;
	}
}
