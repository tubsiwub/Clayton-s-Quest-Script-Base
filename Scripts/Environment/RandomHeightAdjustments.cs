using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomHeightAdjustments : MonoBehaviour {



	void Start () {

		float randomY = Random.Range (transform.position.y - 0.111f, transform.position.y + 0.111f);

		transform.position = new Vector3 (transform.position.x, randomY, transform.position.z);

	}



}

