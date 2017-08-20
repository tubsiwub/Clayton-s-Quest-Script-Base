using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterflyFloat : MonoBehaviour
{
	Rigidbody rb;
	Renderer rend;
	Light goldenLight;

	[SerializeField] Material[] matList;
	[SerializeField] Material yellowMat;
	[SerializeField] GameObject marble;

	[SerializeField] float pushForce = 5;
	[SerializeField] float minVelocity = -2;

	Vector3 startPos;
	Vector3 rotateOrigin;
	int cycles = 0;
	const int maxCycles = 6;

	const float minSpeed = 20;
	float rotateSpeed;
	float rotateDist;

	Transform player;
	bool foundPlayer = false;
	bool canDie = true;
	bool isGolden = false;

	const float dieDistance = 50;
	const float hoverAwayMod = 2.75f;
	const float dirAwayMod = 1.25f;
	const int goldenMarblesToSpawn = 10;

	const float distToFlyAway = 2.4f;
	const float distToFlyAwayMin = 1.6f;    // used for golden butt (fly away should be lower)
	float flyAwayDist;

	void Awake()
	{
		player = GameObject.FindWithTag("Player").transform;
		rb = GetComponent<Rigidbody>();
		goldenLight = GetComponent<Light>();

		startPos = transform.position;
		rend = GetComponentInChildren<Renderer>();
		rend.material = matList[Random.Range(0, matList.Length)];
		flyAwayDist = distToFlyAway;

		rotateDist = Random.Range(0.5f, 2);
		rotateOrigin = startPos += Vector3.right * rotateDist;

		// ensure random number is above min speed (pos or neg)
		while (rotateSpeed < minSpeed && rotateSpeed > -minSpeed)
			rotateSpeed = Random.Range(-45, 45);

		if (rotateSpeed < 0)
			transform.rotation *= Quaternion.Euler(0, 180, 0);

		Color col = rend.material.color;
		rend.material.color = new Color(col.r, col.g, col.b, 0);
		StartCoroutine(FadeIn(0.75f));
	}

	public void Init(bool isGolden)
	{
		this.isGolden = isGolden;

		if (this.isGolden)
		{
			rend.material = yellowMat;
			flyAwayDist = distToFlyAwayMin;
			goldenLight.enabled = true;
		}
	}

	void Update()
	{
		if (!foundPlayer)
			RotateAroundPoint();

		if (!foundPlayer && Vector3.Distance(transform.position, player.position) < flyAwayDist)
		{
			foundPlayer = true;
			Vector3 dirToPlayer = (transform.position - player.position).normalized;
			dirToPlayer.y = 0;
			StartCoroutine(HoverAway(dirToPlayer * dirAwayMod));
		}

		if (canDie && Vector3.Distance(player.position, transform.position) > dieDistance)
		{
			canDie = false;
			StartCoroutine(FadeOutAndDie(2));
		}
	}

	void FixedUpdate()
	{
		if (!foundPlayer)
			Hover();
	}

	void RotateAroundPoint()
	{
		transform.RotateAround(rotateOrigin, Vector3.up, rotateSpeed * Time.deltaTime);
	}

	void Hover()
	{
		if (rb.velocity.y < minVelocity)
		{
			if (cycles % maxCycles == 0)	// every few cycles, reset Y position
			{
				transform.position = new Vector3(transform.position.x, startPos.y, transform.position.z);
			}

			rb.velocity = Vector3.zero;
			rb.AddForce(new Vector3(0, pushForce, 0), ForceMode.VelocityChange);

			cycles++;
		}
	}

	IEnumerator HoverAway(Vector3 dirToPlayer)
	{
		while (true)
		{
			Vector3 addVec = new Vector3(0, pushForce * hoverAwayMod, 0);
			addVec += dirToPlayer;

			rb.AddForce(addVec, ForceMode.VelocityChange);
			yield return new WaitForSeconds(0.5f);
		}
	}

	IEnumerator FadeIn(float speed)
	{
		float a = 0;
		Color col = rend.material.color;
		while (rend.material.color.a < 0.9f)
		{
			a += speed * Time.deltaTime;
			
			rend.material.color = new Color(col.r, col.g, col.b, a);
			yield return null;
		}

		rend.material.color = new Color(col.r, col.g, col.b, 1);
	}

	IEnumerator FadeOutAndDie(float speed)
	{
		float a = 1;
		Color col = rend.material.color;
		while (rend.material.color.a > 0.1f)
		{
			a -= speed * Time.deltaTime;

			rend.material.color = new Color(col.r, col.g, col.b, a);
			yield return null;
		}

		rend.material.color = new Color(col.r, col.g, col.b, 0);
		Destroy(gameObject);
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Player" && isGolden)
		{
			for (int i = 0; i < goldenMarblesToSpawn; i++)
			{
				GameObject obj = Instantiate(marble, transform.position, Quaternion.identity);

				obj.GetComponent<Rigidbody>().velocity += new Vector3(
					Random.Range(-5, 5),
					Random.Range(-5, 5),
					Random.Range(-5, 5)
				);

				obj.GetComponent<Marble>().DelayCollection();
			}

			Destroy(gameObject);
		}
	}
}
