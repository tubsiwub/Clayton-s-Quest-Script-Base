using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorePreviewController : MonoBehaviour
{
	[SerializeField] Animator animator;
	[SerializeField] Renderer[] humanRends;
	[SerializeField] GameObject ballMesh;
	[SerializeField] Transform cam;
	[SerializeField] Transform camPointHuman;
	[SerializeField] Transform camPointBall;

	public delegate void SwitchEvent();
	public event SwitchEvent OnHumanToBall;
	public event SwitchEvent OnHumanToBallFinish;
	public event SwitchEvent OnBallToHuman;
	public event SwitchEvent OnBallToHumanFinish;

	enum PlayerState { Human, Ball };
	PlayerState playerState = PlayerState.Human;
	bool canTransform = true;
	public bool CanTransform { get { return canTransform; } }

	public bool IsHuman { get { return playerState == PlayerState.Human; } }
	public bool IsBall { get { return playerState == PlayerState.Ball; } }

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.K))
			ShowBall();

		if (Input.GetKeyDown(KeyCode.L))
			ShowHuman();
	}

	public void ShowBall()
	{
		if (!canTransform || IsBall) return;

		playerState = PlayerState.Ball;

		MoveCamToBall();

		animator.ResetTrigger("ToBall");
		animator.ResetTrigger("ToHuman");

		animator.SetTrigger("ToBall");
		canTransform = false;

		if (OnHumanToBall != null)
			OnHumanToBall();
	}

	public void ShowHuman()
	{
		if (!canTransform || IsHuman) return;

		playerState = PlayerState.Human;

		MoveCamToHuman();

		animator.ResetTrigger("ToBall");
		animator.ResetTrigger("ToHuman");

		animator.SetTrigger("ToHuman");

		for (int i = 0; i < humanRends.Length; i++)
			humanRends[i].enabled = true;

		ballMesh.SetActive(false);
		canTransform = false;

		if (OnBallToHuman != null)
			OnBallToHuman();
	}

	// called by AnimationEvents. dont call this
	public void EndToBall()
	{
		for (int i = 0; i < humanRends.Length; i++)
			humanRends[i].enabled = false;

		ballMesh.SetActive(true);
		canTransform = true;

		if (OnHumanToBallFinish != null)
			OnHumanToBallFinish();
	}

	// called by AnimationEvents. dont call this
	public void EndToHuman()
	{
		canTransform = true;

		if (OnBallToHumanFinish != null)
			OnBallToHumanFinish();
	}

	void MoveCamToHuman()
	{
		StopAllCoroutines();
		StartCoroutine(FlyToRoutine(cam, camPointHuman.transform.position, camPointHuman.transform.rotation));
	}

	void MoveCamToBall()
	{
		StopAllCoroutines();
		StartCoroutine(FlyToRoutine(cam, camPointBall.transform.position, camPointBall.transform.rotation));
	}

	IEnumerator FlyToRoutine(Transform obj, Vector3 pos, Quaternion rot)
	{
		const float FlySpeed = 3;

		Transform startPoint = obj;
		float dist = Vector3.Distance(obj.position, pos);
		float duration = dist / FlySpeed;
		float t = 0;

		while (Vector3.Distance(obj.position, pos) > 0.01f)
		{
			t += Time.deltaTime / duration;
			obj.position = Vector3.Lerp(startPoint.position, pos, t);
			obj.rotation = Quaternion.Slerp(startPoint.rotation, rot, t);
			yield return null;
		}
	}
}
