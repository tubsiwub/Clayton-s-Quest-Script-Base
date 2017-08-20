using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour {

	// base variables
	protected int health;		// current

	// functions
	public abstract void KillEnemy();
	public abstract void DamageEnemy(int amount);
	public abstract void HealEnemy(int amount);

	void Start () {
		
	}


	void Update () {
		
	}
}
