using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;


public enum TRAVELAREA
{
	HOUSE13 	= 524288,
	HOUSE12 	= 262144,
	HOUSE11 	= 131072,
	HOUSE10 	= 65536,
	HOUSE9 		= 32768,
	HOUSE8 		= 16384,
	HOUSE7 		= 8192,
	HOUSE6 		= 4096,
	HOUSE5 		= 2048,
	HOUSE4 		= 1024,
	HOUSE3 		= 512,
	HOUSE2 		= 256,
	HOUSE1 		= 128,
	INSIDE 		= 64,
	FORBIDDEN 	= 32,
	TRAVERSE 	= 16,
	NPCTOWN 	= 8,
	WALKABLE 	= 1
}


public class NPC_Manager : MonoBehaviour {

	public static NPC_Manager instance = null;

//	List<string> storedNameNPC;

//	public int GetStoredNameCount{ get { return storedNameNPC.Count; } }

	//public TRAVELAREA travelArea = TRAVELAREA.NPCTOWN;
	//public TRAVELAREA insideArea = TRAVELAREA.INSIDE;

	//public GameObject NPCObject;

	Mesh mesh;

	Vector3 specificLoc;

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);
//
//		storedNameNPC = new List<string> ();
//
//		mesh = new Mesh ();
//
//		// assign mesh info to the stored mesh
//		mesh.vertices = NavMesh.CalculateTriangulation ().vertices;
//		mesh.triangles = NavMesh.CalculateTriangulation ().indices;
//
//		// We allow Dennis to always be in the town as a test
//		StoreNPC ("Dennis");
//
//		// Check first, then we check per level load
//		if(SceneManager.GetActiveScene().name == "Level Shell")
//			CheckNPCs ();
//
	}

	void OnEnable(){

		SceneManager.sceneLoaded += OnLevelFinishedLoading;

	}

	void OnDisable(){

		SceneManager.sceneLoaded -= OnLevelFinishedLoading;

	}

	void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode){

		if (scene.name == "Level Shell") {	// scene with the town in it

			GameObject NPCContainer = GameObject.Find ("NPCContainer");

			// Check through all NPCs in the scene
			for (int i = 0; i < NPCContainer.transform.childCount; i++) {

				// Name of the NPC in the scene
				string npcName = NPCContainer.transform.GetChild (i).GetChild (1).GetComponent<NPC_Behavior> ().NPCName;

				// If the NPC we find is stored in our Save Data, set it active
				if (SavingLoading.instance.LoadNPC_String (npcName) != -1) {
					NPCContainer.transform.GetChild (i).gameObject.SetActive (true);
				}
			}
		}

//		for (int i = 0; i < storedNameNPC.Count; i++) {
//
//			if (scene.name != "Level Shell") {
//				return;
//			}
//
//			//StartCoroutine(SpawnNPC (storedNameNPC[i]));		// Spawning isn't working
//
//			CheckNPCs ();
//
//		}
	}

	void CheckNPCs(){
//		
//		GameObject npcContainer;
//		if (GameObject.Find ("NPC Container"))
//			npcContainer = GameObject.Find ("NPC Container");
//		else
//			npcContainer = new GameObject ("NPC Container");
//
//		// turn 'em all off first
//		foreach (Transform npc in npcContainer.transform) {
//
//			npc.transform.gameObject.SetActive (false);
//
//		}
//
//		// turn on the legal ones
//		foreach (string name in storedNameNPC) {
//			foreach (Transform npc in npcContainer.transform) {
//
//				string npcName = npc.GetChild (1).GetComponent<NPC_Behavior> ().NPCName;
//
//				if (storedNameNPC.Contains (npcName)) 
//					npc.transform.gameObject.SetActive (true);
//
//			}
//		}
//
	}

	public void StoreNPC(string nameNPC){
//
//		if (!storedNameNPC.Contains(nameNPC))
//			storedNameNPC.Add (nameNPC);
//		else
//			print ("NPC exists in list already");
//
	}

//	IEnumerator SpawnNPC(string NPCName){
//
//		specificLoc = NavMesh_CalculateSurfaceArea.GetRandomPositionOnMesh(mesh);
//
//		NavMeshHit navHit;
//		NavMesh.SamplePosition (specificLoc, out navHit, 1.0f, NavMesh.AllAreas);
//
//		// Simple check for allowed areas
//		while(navHit.mask != (int)travelArea && navHit.mask != (int)insideArea){
//
//			specificLoc = NavMesh_CalculateSurfaceArea.GetRandomPositionOnMesh(mesh);
//			NavMesh.SamplePosition (specificLoc, out navHit, 1.0f, NavMesh.AllAreas);
//
//			yield return new WaitForEndOfFrame ();
//
//		}
//
//		// Spawns in an NPC into the scene.
//		GameObject spawnedNPC = (GameObject)Instantiate (NPCObject);
//		spawnedNPC.transform.position = specificLoc;						// parent container
//		spawnedNPC.transform.GetChild (0).transform.position = specificLoc;	// destination pointer
//		spawnedNPC.transform.GetChild (1).transform.position = specificLoc;	// npc itself
//		spawnedNPC.transform.GetChild (1).GetComponent<NPC_Behavior>().NPCName = NPCName;
//
//	}

}
