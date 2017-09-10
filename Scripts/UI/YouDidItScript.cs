using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YouDidItScript : MonoBehaviour {

	Animator anim;

	bool useCutscene = false;

	void Start () 
	{
		anim = GetComponent<Animator> ();
	}
	
	void Update () 
	{
		
	}

	public void SetCutsceneCheck(bool check)
	{
		useCutscene = check;
	}

	public void PlayAnimation()
	{
		StartCoroutine (CheckForCutscene ());
	}

	IEnumerator CheckForCutscene()
	{	
		yield return new WaitForSeconds (0.1f);

		while (useCutscene)	// got a cutscene?  Wait for it to end.
			yield return new WaitForEndOfFrame ();
		
		yield return new WaitForSeconds (0.5f);

		if(!useCutscene)
			anim.SetTrigger ("START");
	}
}
