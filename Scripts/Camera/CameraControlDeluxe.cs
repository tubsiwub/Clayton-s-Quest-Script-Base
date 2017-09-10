using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class CameraControlDeluxe : MonoBehaviour
{
	[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
	public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
	[DllImport("user32.dll")]
	public static extern bool SetCursorPos(int X, int Y);

	//Mouse actions
	private const int MOUSEEVENTF_LEFTDOWN = 0x02;
	private const int MOUSEEVENTF_LEFTUP = 0x04;
	private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
	private const int MOUSEEVENTF_RIGHTUP = 0x10;

	float distanceAway = 5;
	float distanceUp = 2;
	float distanceFrom = 4;
	const float ScrollSpeed = 6;

	const float xSpeed = 60;
	const float ySpeed = 5;
	const float speedMod = 2;		// needed to (hopefully) keep movement speed similar between mouse and stick

	const float MAX_SLOW_TIME = 0.09f;

	public float CameraSpeed { set; get; }

	Quaternion lastFollowRot;
	Vector3 storedWallHitPoint;

	Transform follow;
	PlayerFloorGetter floorGetter;
	PlayerHandler playerHandler;
	BallController ballController;

	Vector3 lastPlayerPos;
	Vector3 lookTarget = Vector3.zero;
	Transform cameraSetPoint;
	bool HasCameraSetPoint { get { return cameraSetPoint != null; } }
	bool HasLookTarget { get { return lookTarget != Vector3.zero; } }
	IEnumerator lookRoutineRef;

	[SerializeField] Transform player;
	Transform rotateMesh;
	bool setCamBehindPlayer = false;

	[SerializeField] bool invertX;
	[SerializeField] bool invertY;
	public bool InvertX { set { invertX = value; } get { return invertX; } }
	public bool InvertY { set { invertY = value; } get { return invertY; } }

	[SerializeField] float amount = 6;
	[SerializeField] bool doAutoRotate = false;
	public bool AutoRotate { set { doAutoRotate = value; } get { return doAutoRotate; } }

	static int cursorLockFrame = 0;
	public static int CursorLockFrame { get { return cursorLockFrame; } }

	public delegate void CamFinish();
	public event CamFinish OnBehindPlayer;

	bool camFrozen = false;
	public void SetFreeze(bool set, bool setCursor = true)
	{
		camFrozen = set;

		if (setCursor)
		{
			if (!set)
				ForceCursorLock();  // only force lock (click) when we are UNFREEZING the camera

			SetCursorLock();
		}
	}

	public void ForceCursorLock()
	{
		if (Application.isEditor)
		{
			if (!CursorOnScreen()) return;
		}
		else
			SetCursorPos(Screen.width/2, Screen.height/2);

		cursorLockFrame = Time.frameCount;
		Cursor.lockState = CursorLockMode.Locked;
		mouse_event(MOUSEEVENTF_LEFTUP | MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
	}

	bool CursorOnScreen()
	{
		return (Input.mousePosition.x > 0 && Input.mousePosition.x < Screen.width &&
			Input.mousePosition.y > 0 && Input.mousePosition.y < Screen.height);
	}

	void Awake()
	{
		Cursor.lockState = CursorLockMode.Locked;
		StartCoroutine(WaitThenTurnOn());
	}

	IEnumerator WaitThenTurnOn()	// we have to let PlayerFloorGetter run for 1 whole frame before we start up
	{
		yield return null;
		this.enabled = true;
	}

	void Start()
	{
		CameraSpeed = 1;
		follow = GameObject.Find("Follow").transform;
		floorGetter = player.gameObject.GetComponent<PlayerFloorGetter>();
		rotateMesh = player.GetComponent<PlayerHandler>().RotateMesh;

		// temp because I'm tired
		playerHandler = player.GetComponent<PlayerHandler>();
		ballController = player.GetComponent<BallController>();
	}

	void Update()
	{
		if (!PlayerHandler.CanUpdate) return;

		if (!camFrozen)
			SetCursorLock();

		if (Input.GetButtonDown("CameraBehind") && !setCamBehindPlayer)
			StartCoroutine(CamBehindPlayer());
	}

	void SetCursorLock()
	{
		bool anyClickForm = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);

		if (anyClickForm && !camFrozen)
			Cursor.lockState = CursorLockMode.Locked;

		if (camFrozen && Cursor.lockState == CursorLockMode.Locked)
			Cursor.lockState = CursorLockMode.None;

		Cursor.visible = Cursor.lockState != CursorLockMode.Locked;
	}

	void OnGUI()
	{
		Event scrollEvent = Event.current;

		if (scrollEvent.type == EventType.ScrollWheel)
			distanceFrom += scrollEvent.delta.y / ScrollSpeed;

		distanceFrom = Mathf.Clamp(distanceFrom, -2, 4);
	}

	void LateUpdate()
	{
		if (camFrozen || Cursor.lockState != CursorLockMode.Locked || Time.deltaTime > MAX_SLOW_TIME || !PlayerHandler.CanUpdate)
		{
			if (HasCameraSetPoint)
			{
				SetToDirectPos();
				return;
			}

			follow.rotation = Quaternion.Euler(0, lastFollowRot.eulerAngles.y, 0);
			return;  // only move camera when cursor is focused
		}

		Vector3 characterOffset = floorGetter.PlayerFloor + new Vector3(0, 3, 0);

		Vector2 input = new Vector2(Input.GetAxisRaw("CamHorizontal"), Input.GetAxisRaw("CamVertical"));
		Vector2 movement = new Vector2(0, 0);
		if (!HasLookTarget) movement.x = input.x * ((xSpeed * CameraSpeed * speedMod) * Time.deltaTime) * (invertX ? -1 : 1);
		movement.y = input.y * (ySpeed * CameraSpeed * speedMod) * (invertY ? -1 : 1);

		distanceUp += movement.y * Time.deltaTime;
		distanceUp = Mathf.Clamp(distanceUp, -3, 6);

		distanceAway -= (movement.y / 2) * Time.deltaTime;
		distanceAway = Mathf.Clamp(distanceAway, 3.5f, 6);

		follow.rotation = Quaternion.Euler(0, lastFollowRot.eulerAngles.y + movement.x + GetAutoRotateAmount(), 0);

		Vector3 targetPosition = FindTargetPosition(characterOffset, setCamBehindPlayer);

		if (!HasCameraSetPoint)
		{
			transform.position = WallStuff(characterOffset, targetPosition);
			transform.LookAt(characterOffset);

			lastFollowRot = follow.rotation;
			lastPlayerPos = floorGetter.PlayerFloor; lastPlayerPos.y = 0;
		}
		else SetToDirectPos();
	}

	void SetToDirectPos()
	{
		follow.localRotation = Quaternion.identity;
		lastFollowRot = follow.rotation;

		transform.position = cameraSetPoint.position;
		transform.rotation = cameraSetPoint.rotation;
	}

	Vector3 FindTargetPosition(Vector3 characterOffset, bool camBehindPlayer)
	{
		Vector3 dirTarget = characterOffset;
		if (HasLookTarget) dirTarget = lookTarget;

		Vector3 curLookDir = dirTarget - transform.position;
		if (camBehindPlayer)
		{
			if (playerHandler.CurrentState != PlayerHandler.PlayerState.Ball)
				curLookDir = rotateMesh.forward - Vector3.up;
			else
			{
				Vector3 dir = ballController.Distance; dir.y = 0;

				if (dir.normalized.magnitude >= 0.75f)
					curLookDir = dir - Vector3.up;
			}
		}

		curLookDir.y = 0;

		Vector3 targetPosition = (characterOffset + follow.up * distanceUp) -
			(curLookDir.normalized * distanceAway) - (transform.forward * distanceFrom);

		return targetPosition;
	}

	public void SetLookTarget(Vector3 target, float lookSpeed)
	{
		lookRoutineRef = SmoothToLookTarget(target, lookSpeed);
		StartCoroutine(lookRoutineRef);
	}

	public Vector3 GetLookTarget() 
	{
		return lookTarget;
	}

	public void CancelLookTarget()
	{
		if (lookRoutineRef != null)
			StopCoroutine(lookRoutineRef);

		lookTarget = Vector3.zero;
	}

	IEnumerator SmoothToLookTarget(Vector3 target, float speed)
	{
		if (floorGetter == null)
			yield return null;

		Vector3 characterOffset = floorGetter.PlayerFloor + new Vector3(0, 3, 0);
		lookTarget = characterOffset;
		while (Vector3.Distance(lookTarget, target) > 2)
		{
			lookTarget = Vector3.Lerp(lookTarget, target, speed * Time.deltaTime);
			yield return null;
		}

		lookTarget = target;
	}

	public void SetToPlayer()
	{
		floorGetter.SetToPlayer();
	}

	public void DoCamBehindPlayer()
	{
		StartCoroutine(CamBehindPlayer());
	}

	IEnumerator CamBehindPlayer()
	{
		setCamBehindPlayer = true;
		distanceUp = 2;
		distanceAway = 5;
		distanceFrom = 4;

		for (int i = 0; i < 8; i++)	// it takes like, 8 frames or whatever to make it behind the player
			yield return null;

		setCamBehindPlayer = false;

		if (OnBehindPlayer != null)
			OnBehindPlayer();
	}

	public void SetPointDirect(Transform point)
	{
		cameraSetPoint = point;
	}

	public void CancelPointDirect()
	{
		cameraSetPoint = null;
	}

	float GetAutoRotateAmount()
	{
		if (!doAutoRotate) return 0;

		Vector3 pos = floorGetter.PlayerFloor; pos.y = 0;
		Vector3 distance = pos - lastPlayerPos;
		float dot = Vector3.Dot(distance.normalized, transform.right);
		float dist = Vector3.Distance(pos, lastPlayerPos) * dot;

		//return dist * (6 + (GetMagicNum() * amount));
		return dist * (amount + GetMagicNum());
	}

	float GetMagicNum()
	{
		//Vector3 forlard = transform.forward; forlard.y = 0;
		//float magicNum = Mathf.Abs(1 - (Vector3.Dot(transform.forward, forlard)));
		//print(magicNum);
		//
		//return magicNum;

		Vector3 play = GameObject.FindWithTag("Player").transform.position; play.y = 0;
		Vector3 me = transform.position; me.y = 0;
		float magicNum = Mathf.Abs(10 - Vector3.Distance(play, me));


		return magicNum;
	}

	/*void OldNonsense()
	{
		//float num = Mathf.Abs((distanceFrom - 10));
		//float mod = (num / 6) - 1;
		//mod *= 4;
		//num = num + mod;
	}*/

	// stolen from Tristan's CameraControl
	// changed any 'return transform.position' to 'return targetPos' (set position directly to targetPos, if not wall colliding)
	// this may change later, idk
	Vector3 WallStuff(Vector3 charOffset, Vector3 targetPos)
	{
		RaycastHit hit;

		if (distanceUp >= 0)
		{
			// If the camera is level or above the ground
			if (Physics.Linecast(charOffset, targetPos, out hit, -1, QueryTriggerInteraction.Ignore))
			{
				if (hit.transform.GetComponent<Camera_IgnoreObject>() || hit.transform.GetComponent<Collider>().isTrigger)
					return targetPos;

				if (hit.point.y < targetPos.y)
					storedWallHitPoint = new Vector3(hit.point.x, hit.point.y, hit.point.z);

				return new Vector3(hit.point.x, storedWallHitPoint.y, hit.point.z);
			}
		}
		else
		{
			// If the camera was moved below waist level
			if (Physics.Linecast(charOffset, targetPos - new Vector3(0, 0.12f, 0), out hit, - 1, QueryTriggerInteraction.Ignore))
			{
				// shift camera's position check down a tad
				if (hit.transform.GetComponent<Camera_IgnoreObject>() || hit.transform.GetComponent<Collider>().isTrigger)
					return targetPos;

				// position camera based off of every value of the ray instead of just x and z
				return new Vector3(hit.point.x, hit.point.y, hit.point.z) + ((follow.position - targetPos) * 0.03f);
			}
		}

		return targetPos;
	}
}
