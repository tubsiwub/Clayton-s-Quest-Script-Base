using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindmillScript : MonoBehaviour 
{
	Animator anim;

	void Start ()
	{
		anim = GetComponent<Animator>();
		anim.SetBool("Rotate", true);
	}
}
