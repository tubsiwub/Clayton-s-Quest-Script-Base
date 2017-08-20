using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// make ball only move horizontal when pushed

public class PushyBall : MonoBehaviour {

	public float PunchForce = 48.0f;

	public float PushForce = 1.2f;

	GameObject rightHand, leftHand;
	Rigidbody rb;
	Animator humanAnim;
	PlayerAttack playerAttack;
	Transform rotateMesh;

	// have some leniance so the player can leave ball trigger while still pushing for a bit
	int pushFrameStamp = 0;
	const int pushAllowanceFrames = 20;
	const float facingObject = 0.6f;        // lower is more lenient
	bool isFacingThisFrame = false;
	bool playerPushing = false;

	void Start () {

		rightHand = GameObject.FindWithTag ("Player").GetComponent<PlayerAttack> ().RightHand;
		leftHand = GameObject.FindWithTag ("Player").GetComponent<PlayerAttack> ().LeftHand;
		playerAttack = GameObject.FindWithTag ("Player").GetComponent<PlayerAttack> ();
		rb = GetComponent<Rigidbody>();
		humanAnim = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>().HumanAnimator;
		rotateMesh = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>().RotateMesh;
	}

	void Update () {

		isFacingThisFrame = FacingCloseEnough();

		if (!playerPushing) return;

		if (humanAnim.isInitialized && Time.frameCount > pushFrameStamp + pushAllowanceFrames)
		{
			humanAnim.SetBool("Pushing", false);
			playerPushing = false;
		}

		// just in case
		if (playerAttack.IsAttacking == true || !isFacingThisFrame)
		{
			humanAnim.SetBool("Pushing", false);
			playerPushing = false;
		}

	}

	public void RemoveAllReferences(){
		
		GameObject[] buttonObjList = GameObject.FindGameObjectsWithTag ("ResetBallButton");

		for (int i = 0; i < buttonObjList.Length; i++) {
			if (buttonObjList [i].transform.GetChild (1).GetComponent<PuzzleButton> ().keyObj == this.gameObject)
				buttonObjList [i].transform.GetChild (1).GetComponent<PuzzleButton> ().ResetKeyObj (this.gameObject);
		}
	}

	bool FacingCloseEnough() {

		Vector3 direction = (transform.position - rotateMesh.position);
		direction.y = 0;
		return Vector3.Dot(direction.normalized, rotateMesh.forward) > facingObject;
	}

	void OnTriggerEnter(Collider col) {

		if (col.gameObject == rightHand || col.gameObject == leftHand) {

			StartCoroutine (PunchBall (0.25f, col));

		}

	}

	void OnTriggerStay(Collider col){

		if (col.transform.tag == "Player") {

			Vector3 direction = this.transform.position - col.transform.position;
			direction = direction.normalized;
			direction.y = 0;
			rb.AddForce (direction * PushForce, ForceMode.Force);

			if (humanAnim.isInitialized && playerAttack.IsAttacking == false && isFacingThisFrame)
			{
				// make sure we don't do animation while standing on top of ball
				bool belowBall = col.transform.position.y < transform.position.y;
				humanAnim.SetBool("Pushing", belowBall);
				pushFrameStamp = Time.frameCount;   // make sure our framestamp is up-to-date!
				playerPushing = true;
			}
		}

	}

	IEnumerator PunchBall(float delayTime, Collider col) {

		yield return new WaitForSeconds (delayTime);

		Vector3 direction = this.transform.position - col.transform.position;
		direction = direction.normalized;
		direction.y = 0;
		rb.AddForce (direction * PunchForce, ForceMode.Force);

	}

}
