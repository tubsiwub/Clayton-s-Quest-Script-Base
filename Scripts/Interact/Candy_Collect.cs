using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Candy_Collect : MonoBehaviour {

	//[SerializeField] float heightFromFloor = 1;

	// Events
	public delegate void Candy_Collected();
	public static event Candy_Collected OnCandyCollect; // - fire when collected

	void Start () 
	{
		// candy will be deleted if necessary before this gets called
		StartCoroutine(WaitAFrame());
	}

	IEnumerator WaitAFrame()
	{
		yield return null;

		SavingLoading.instance.SaveStoredCandy (this.gameObject, false);
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.transform.tag == "Player") 
		{
			if (OnCandyCollect != null)
				OnCandyCollect ();

			// in SavingLoading, mark this candy as 'Collected' - store using name
			SavingLoading.instance.SaveStoredCandy (this.gameObject, true);

			// kill candy after saving it's fate
			Destroy(this.gameObject);

		}
	}

}
