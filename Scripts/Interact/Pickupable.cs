using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LedgeGrabIgnore))]
public class Pickupable : MonoBehaviour
{
	[SerializeField] Vector3 holdOffset;
	[SerializeField] float throwForceMultiplier = 1;

	const float range = 1.85f;
	const float flySpeed = 10;
	const float facingObject = 0.4f;        // lower is more lenient
	const float deathY = -50;

	Collider col;
	Rigidbody rb;
	Transform player;
	PlayerHolder playerHolder;
	Transform rightHand = null;

	Vector3 startPos;
	Quaternion startRot;

	Renderer rend;
	Color startCol;
	Color glowCol = Color.green;
	bool gettingPickedUp = false;
	bool onList = false;

	public bool IsBeingHeld { get { return playerHolder.HeldObject == this; } }
	bool CloseToPlayer { get { return Vector3.Distance(transform.position, player.position) < range; } }
	bool IsGlowing { get { return rend.material.color == glowCol; } }
	bool HasRefs { get { return playerHolder != null && player != null; } }

	ObjInfo info;

	void Awake(){

		if(GetComponent<ObjInfo> ())
			info = GetComponent<ObjInfo> ();

	}

	void Start()
	{
		Collider[] cols = GetComponents<Collider>();
		for (int i = 0; i < cols.Length; i++)
		{
			if (!cols[i].isTrigger)
			{
				col = cols[i];
				break;
			}
		}

		rb = GetComponent<Rigidbody>();
		rend = GetComponent<Renderer>();
		startCol = rend.material.color;

		startPos = transform.position;
		startRot = transform.rotation;

		SceneLoaded ();

		StartCoroutine(FindPlayerHolder());
	}

	void SceneLoaded(){

		if(info)
			info.LOAD();

	}

	void SaveObject(bool overwrite, bool exists){

		if(info)
			info.SAVE (overwrite, exists);
		SavingLoading.instance.SaveData();
	}

	public void Respawn()
	{
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;

		transform.position = startPos;
		transform.rotation = startRot;
	}

	IEnumerator FindPlayerHolder()
	{
		while (!HasRefs)
		{
			playerHolder = GameObject.FindWithTag("Player").GetComponentInChildren<PlayerHolder>();
			if (playerHolder != null) player = playerHolder.transform;
			
			yield return new WaitForSeconds(0.4f);	// don't bog anything down, only check sometimes
		}
	}

	void OnDestroy()
	{
		if (!HasRefs) return;

		playerHolder.RemoveFromList(this);
		if (IsBeingHeld) playerHolder.Drop();
	}

	void OnDisable()
	{
		if (!HasRefs) return;

		playerHolder.RemoveFromList(this);
		if (IsBeingHeld) playerHolder.Drop();
	}

	bool FacingCloseEnough()
	{
		Vector3 direction = (transform.position - player.position);
		direction.y = 0;
		return Vector3.Dot(direction.normalized, player.forward) > facingObject;
	}

	void Update()
	{
		if (!HasRefs) return;

		bool facingCloseEnough = FacingCloseEnough();

		if (CloseToPlayer && !onList && facingCloseEnough)
		{
			playerHolder.AddToList(this);
			onList = true;
		}
		if ((!CloseToPlayer || !facingCloseEnough) && onList)
		{
			playerHolder.RemoveFromList(this);
			onList = false;
		}

		if (transform.position.y < deathY)
			Respawn();
	}

	public void CustomUpdate()
	{
		if (!IsBeingHeld && !IsGlowing)
			Glow();

		if (rightHand != null)
		{
			float forward = holdOffset.z;
			float right = holdOffset.x;
			float up = holdOffset.y;
			transform.position = rightHand.position + (player.forward * forward) + (player.right * right) + (player.up * up);
		}

		if (Input.GetButtonDown("Interact") && !gettingPickedUp)
		{
			SaveObject (true, true);

			if (IsBeingHeld)
				playerHolder.Drop();
			else
				PickUp();
		}
	}

	public void Glow()
	{
		rend.material.color = glowCol;
	}

	public void StopGlow()
	{
		rend.material.color = startCol;
	}

	void PickUp()
	{
		if (holdOffset == Vector3.zero)
		{
			string warning = "Hi! This " + gameObject.name + " still has a default holdOffset of (0, 0, 0), ";
			warning += "which probably isn't right. Don't forget to set the holdOffset!";
			Debug.LogWarning(warning);
		}

		playerHolder.SetHolding(this);
		StopGlow();

		gettingPickedUp = true;
		rb.isKinematic = true;
		transform.rotation = player.rotation;
	}

	public void SetHandPos(Transform rightHand)
	{
		transform.parent = playerHolder.transform;
		col.enabled = false;
		this.rightHand = rightHand;
	}

	public void PickupAnimationDone()
	{
		gettingPickedUp = false;
	}

	public void PutDown(bool disableCollider)
	{
		transform.parent = null;
		rightHand = null;

		rb.isKinematic = false;
		if (disableCollider) col.enabled = true;

		gettingPickedUp = false;

		SaveObject (true, true);
	}

	IEnumerator EnableColliderAfter(int frames)
	{
		for (int i = 0; i < frames; i++)
			yield return null;

		col.enabled = true;
	}

	public void Thow(Vector3 direction, float force)
	{
		PutDown(false);
		StartCoroutine(EnableColliderAfter(7));

		force *= throwForceMultiplier;
		Vector3 upForce = Vector3.up * 3 * (force * 0.25f);
		Vector3 forwardForce = direction * force;

		rb.AddForce(upForce + forwardForce, ForceMode.VelocityChange);
	}

	// Save whenever this object hits something; so throwing matters.
	void OnCollisionEnter()
	{
		SaveObject (true, true);
	}
}
