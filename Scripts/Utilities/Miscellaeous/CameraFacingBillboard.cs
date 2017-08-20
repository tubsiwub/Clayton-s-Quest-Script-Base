// This is basically the same script as InteractPopup_FacePlayer

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFacingBillboard : MonoBehaviour
{
	[SerializeField] float offsetAngle = 0;
	[SerializeField] bool trackXRotation = true;

	Transform cam;
	PlayerHandler playerHandler;

	void Start()
	{
		cam = Camera.main.transform;
		playerHandler = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>();
	}

	// always refresh late update
	void LateUpdate()
	{
		FaceCamera();
	}

	// only refresh fixed if physics controlled (ball, slider)
	void FixedUpdate()
	{
		if (playerHandler.IsPhysicsControlled)
			FaceCamera();
	}

	void FaceCamera()
	{
		Quaternion camRotation = cam.rotation;
		if (!trackXRotation) camRotation = Quaternion.Euler(0, camRotation.eulerAngles.y, camRotation.eulerAngles.z);

		transform.LookAt(transform.position + camRotation * Vector3.forward, camRotation * Vector3.up);
		transform.Rotate(0, offsetAngle, 0);
	}
}
