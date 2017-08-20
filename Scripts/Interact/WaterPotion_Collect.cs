using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPotion_Collect : MonoBehaviour {

	public ParticleSystem splashParticleSystem;

	Animator anim;

	void Start () {

		anim = GetComponent<Animator> ();

	}

	void Update () {

	}

	void OnTriggerEnter(Collider col){

		if(col.transform.tag == "Player")
			CollectBottle (col.transform.gameObject);

	}

	void CollectBottle(GameObject playerObj){

		// heal player
		HealthManager.instance.RegainLives (1);

		// spawn emitter / effects
		StartCoroutine (SplashEmitter ());

	}

	IEnumerator SplashEmitter(){

		splashParticleSystem.transform.parent = null;
		splashParticleSystem.Play ();

		anim.SetTrigger ("Collect");

		this.GetComponent<Collider> ().enabled = false;

		yield return new WaitForSeconds (2.0f);

		// destroy bottle
		Destroy(splashParticleSystem.gameObject);
		Destroy (this.gameObject);

	}

}

