using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest_Storage : MonoBehaviour {

	public static Quest_Storage instance = null;



	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

	}

	// Update is called once per frame
	void Update () {
		
	}
}
