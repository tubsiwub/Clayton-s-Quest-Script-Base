using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPC_MenuCount : MonoBehaviour {

	Text text;

	void Start () 
	{
		text = GetComponent<Text> ();
	}
	
	void Update () 
	{
		text.text = SavingLoading.instance.GetNPCCount ().ToString ();
	}
}
