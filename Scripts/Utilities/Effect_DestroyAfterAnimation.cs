using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_DestroyAfterAnimation : MonoBehaviour {

	Animator anim;

	public string tagName;

	void Start () 
	{
		anim = GetComponent<Animator> ();
	}

	void Update () 
	{
		if (anim.GetCurrentAnimatorStateInfo (0).IsTag (tagName)) 
		{
			Destroy (this.transform.gameObject);
		}
	}
}
