// Tristan
// This is basically the same script as CameraFacingBillboard

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractPopup_FacePlayer : MonoBehaviour {

	[SerializeField] Vector3 YOffset = Vector3.zero;
	Transform cam;

	PlayerHandler playerHandler;

	void Start () {

		cam = Camera.main.transform;
		playerHandler = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>();

	}

	// always refresh late update
	void LateUpdate() {

		FaceCamera();

	}

	// only refresh fixed if physics controlled (ball, slider)
	void FixedUpdate() {

		if (playerHandler.IsPhysicsControlled)
			FaceCamera();

	}

	void FaceCamera() {

		Quaternion camRotation = cam.rotation;

		transform.LookAt(transform.position + camRotation * Vector3.forward, camRotation * Vector3.up);
		transform.Rotate(YOffset.x, YOffset.y, YOffset.z);

	}
}
