using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Placement:  Place script on any Enemy that uses these states

// Purpose:  Contains the states that are referenced in any other enemy behavior scripts.
//				Also contains the triggers behind state changes.

public class Enemy_States : MonoBehaviour {

	public enum EnemyStates{

		WANDER,
		CHASE,
		ATTACK,
		DEAD

	}

	[SerializeField]
	EnemyStates enemyStates;

	string currentState = "wander";

	public string STATE { 
		get { return currentState; } 
		set { 
			
			switch (value.ToLower()) {

			case "wander":
				enemyStates = EnemyStates.WANDER;
				break;
			case "attack":
				enemyStates = EnemyStates.ATTACK;
				break;
			case "chase":
				enemyStates = EnemyStates.CHASE;
				break;
			case "dead":
				enemyStates = EnemyStates.DEAD;
				break;

			}

			currentState = value;
		}}

	void Start () {
		
	}

	void Update () {

//		if (Input.GetKeyDown (KeyCode.W))
//			enemyStates = EnemyStates.WANDER;
//
//		if (Input.GetKeyDown (KeyCode.C)) {
//			
//			enemyStates = EnemyStates.CHASE;
//
//			if(GetComponent<Enemy_Movement> ())
//				if(GetComponent<Enemy_Movement> ().enabled)
//					GetComponent<Enemy_Movement> ().ENABLE_NAVAGENT ();
//		}
//
//		if (Input.GetKeyDown (KeyCode.A))
//			enemyStates = EnemyStates.ATTACK;

		switch (enemyStates) {

		case EnemyStates.WANDER:
			if(currentState != "wander") currentState = "wander";
			break;

		case EnemyStates.CHASE:
			if(currentState != "chase") currentState = "chase";
			break;

		case EnemyStates.ATTACK:
			if(currentState != "attack") currentState = "attack";
			break;

		case EnemyStates.DEAD:
			if(currentState != "dead") currentState = "dead";
			break;
		}
	}
}
