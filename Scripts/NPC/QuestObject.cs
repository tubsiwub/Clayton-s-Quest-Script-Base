using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// QuestObject fires off an event when defeated / turned in

public enum QUESTOBJECTTYPE {

	collectible,		// specific quest object
	basicenemy,			// just an enemy that can be defeated for a quest
	bossenemy			// specific enemy for a quest

}

public enum COLLECTIBLETYPE {

	coconut

}


public class QuestObject : MonoBehaviour {


	QUEST_STATUS questStatus = QUEST_STATUS.NEUTRAL;
	public QUEST_STATUS QuestStatus { get { return questStatus; } set { questStatus = value; } } 


	public QUESTOBJECTTYPE questObjectType;
	public COLLECTIBLETYPE collectibleType;

	// Events
	public delegate void QuestObject_TurnIn();
	public event QuestObject_TurnIn OnQuestObject;						// - fire when turned in

	public delegate void QuestObject_EnemyDefeat();
	public static event QuestObject_EnemyDefeat OnQuestEnemyDefeat;		// fire when killed

	public delegate void QuestObject_BossDefeat();
	public static event QuestObject_BossDefeat OnQuestBossDefeat;		// fire when killed

	public GameObject objectWithMaterial;

	public Material inactiveMaterial, activeMaterial;

	public bool destroyOnFinish;
	string storageKey = "";

	void Start () {
		
		if (GetComponent<SavingLoading_StorageKeyCheck> ()) {
			storageKey = GetComponent<SavingLoading_StorageKeyCheck> ().storageKey;

			// Depending on load check, remove object based on status
			if (!destroyOnFinish) {
				
				if (SavingLoading.instance.CheckStorageKeyStatus (storageKey))	// COMPLETE - general check; non specific
					Destroy (this.gameObject);
				
			} else {
				
				if (SavingLoading.instance.LoadQuestStatus_Container(storageKey) == QUEST_STATUS.FINISHED)	// FINISHED
				Destroy (this.gameObject);

			}

		}
		
	}



	void Update () {

		if (questStatus != QUEST_STATUS.NEUTRAL && questStatus != QUEST_STATUS.FAILED) {

			if (questObjectType == QUESTOBJECTTYPE.bossenemy || questObjectType == QUESTOBJECTTYPE.basicenemy) {

				SetActive (true);

			}

		} else {

			if (questObjectType == QUESTOBJECTTYPE.bossenemy || questObjectType == QUESTOBJECTTYPE.basicenemy) {

				SetActive (false);

			}

		}

	}

	void SetActive(bool state){

		// General
		if(GetComponent<CapsuleCollider> ())
			GetComponent<CapsuleCollider> ().enabled = state;
		
		if(GetComponent<SphereCollider> ())
			GetComponent<SphereCollider> ().enabled = state;

		if (GetComponent<Enemy_States> ())
			GetComponent<Enemy_States> ().enabled = state;
		
		if (GetComponent<SmallEnemyControl> ())
			GetComponent<SmallEnemyControl> ().enabled = state;

		if (GetComponent<FlyingEnemyController> ())
			GetComponent<FlyingEnemyController> ().enabled = state;

		if (GetComponent<NewSmallEnemyScript> ())
			GetComponent<NewSmallEnemyScript> ().enabled = state;

		if (GetComponent<Rigidbody> ())
			GetComponent<Rigidbody> ().detectCollisions = state;

		if (GetComponent<Rigidbody> ())
			GetComponent<Rigidbody> ().useGravity = state;

		if (GetComponent<Rigidbody> ())
			GetComponent<Rigidbody> ().isKinematic = !state;
		
		// Always stay true...
		if (GetComponent<NavMeshAgentScript> ())
			GetComponent<NavMeshAgentScript> ().enabled = true;

		if (GetComponent<NavMeshAgent> ())
			GetComponent<NavMeshAgent> ().enabled = true;


		if (state) {

			if (objectWithMaterial.GetComponent<Renderer> ()
				&& GetComponent<NewSmallEnemyScript>().isDamaged == false)
				objectWithMaterial.GetComponent<Renderer> ().sharedMaterial = activeMaterial;
			
		} else {

			if (objectWithMaterial.GetComponent<Renderer> () 
				&& GetComponent<NewSmallEnemyScript>().isDamaged == false)
				objectWithMaterial.GetComponent<Renderer> ().sharedMaterial = inactiveMaterial;

		}

	}

	public void Collected(){
		
		// fire event
		if (OnQuestObject != null)
			OnQuestObject ();
		
	}

	public void Defeated(string type){

		// basic enemy check
		if (type.ToLower () == "basic" 
			|| type.ToLower () == "small" 
			|| type.ToLower () == "dust" 
			|| type.ToLower () == "dustbunny") {
			
			// fire event
			if (OnQuestEnemyDefeat != null)
				OnQuestEnemyDefeat ();
			
		}

		// boss enemy check
		if (type.ToLower () == "boss" 
			|| type.ToLower () == "large") {

			// fire event
			if (OnQuestBossDefeat != null)
				OnQuestBossDefeat ();

		}
		
	}



}

