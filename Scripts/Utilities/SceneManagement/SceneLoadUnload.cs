using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SceneLoadUnload : MonoBehaviour
{
	[SerializeField] string[] loadNames;
	[SerializeField] string[] unloadNames;

	Collider col;
	bool reEnableTrigger = true;

	void Awake()
	{
		col = GetComponent<Collider>();
	}

	void OnDrawGizmos()
	{
		if (col == null) col = GetComponent<Collider>();

		Gizmos.color = Color.green;
		Gizmos.DrawCube(col.bounds.center, col.bounds.extents * 2);
		Gizmos.DrawWireCube(col.bounds.center, col.bounds.extents * 2);
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag != "Player") return;
		if (!reEnableTrigger) return;	// make sure trigger doesn't happen a bunch

		for (int i = 0; i < loadNames.Length; i++)
			if (loadNames[i] != "") LevelManager.instance.Load(loadNames[i]);

		for (int i = 0; i < unloadNames.Length; i++)
			if (unloadNames[i] != "") LevelManager.instance.Unload(unloadNames[i]);

		reEnableTrigger = false;
	}

	void OnTriggerExit()
	{
		if (col.gameObject.tag != "Player") return;
		reEnableTrigger = true;
	}
}
