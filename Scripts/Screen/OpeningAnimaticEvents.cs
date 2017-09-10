using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningAnimaticEvents : MonoBehaviour
{
	const float CircleOutSpeed = 0.65f;

	Transform cameraSetPoint;
	CameraControlDeluxe cam;
	ScreenTransition st;
	PlayerHandler playerHandler;
	GameObject pauseMenu;

	int screenTransDoneFrame = 0;

	void Start()
	{
		GameObject foundCam = Camera.main.gameObject;
		cam = foundCam.GetComponent<CameraControlDeluxe>();
		st = foundCam.GetComponent<ScreenTransition>();
		playerHandler = GameObject.FindWithTag("Player").GetComponent<PlayerHandler>();
		cameraSetPoint = GameObject.Find("CameraSetPoint").transform;

		pauseMenu = GameObject.Find("In-Game Menus(Clone)");
		pauseMenu.SetActive(false);

		ScreenTransition.OnDoneBackward += ScreenTransitionDone;
		HumanController.OnHumanLanded += UnfreezeCam;
	}

	void RemoveEvents()
	{
		ScreenTransition.OnDoneBackward -= ScreenTransitionDone;
		HumanController.OnHumanLanded -= UnfreezeCam;
	}

	void Update()
	{
		if (Input.GetButtonDown("Abort"))
			FinishEverything();
	}

	public void AnimationFinished()
	{
		GetComponent<CanvasGroup>().alpha = 0;

		st.SetDirectly(1, "black_pattern");
		st.Backward(0.5f, "circle_pattern");

		cam.SetPointDirect(cameraSetPoint);


		StartCoroutine(WaitForTransition());
	}

	void ScreenTransitionDone()
	{
		screenTransDoneFrame = Time.frameCount;
	}

	IEnumerator WaitForTransition()
	{
		while (screenTransDoneFrame != Time.frameCount)
		{
			yield return null;
		}
		
		UnfreezePlayer();
	}

	void UnfreezePlayer()
	{
		playerHandler.SetFrozen(false, false);
		playerHandler.SetHorizontalFrozen(true);

		Vector3 rot = -cameraSetPoint.forward; rot.y = 0;
		playerHandler.RotateMesh.forward = rot;
	}

	void UnfreezeCam()
	{
		StartCoroutine(WaitThenUnfreezeCam());
	}

	IEnumerator WaitThenUnfreezeCam()
	{
		const float FreezeTime = 0.5f;
		yield return new WaitForSeconds(FreezeTime);

		cam.SetFreeze(false);
		cam.CancelPointDirect();
		pauseMenu.SetActive(true);
		playerHandler.SetHorizontalFrozen(false);

		RemoveEvents();
		Destroy(gameObject);
	}

	void FinishEverything()
	{
		StopAllCoroutines();

		playerHandler.SetFrozen(false, false);
		playerHandler.SetHorizontalFrozen(false);

		st.SetDirectly(0, "black_pattern");
		cam.SetFreeze(false);
		cam.CancelPointDirect();
		pauseMenu.SetActive(true);

		RemoveEvents();
		Destroy(gameObject);
	}
}
