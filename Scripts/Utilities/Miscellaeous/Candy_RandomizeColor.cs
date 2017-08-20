using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candy_RandomizeColor : MonoBehaviour {

	public List<Material> randomMaterials_candy;
	public List<Material> randomMaterials_sides;
	public List<Material> randomMaterials_wrapper;

	void Awake () {

		int randomNum = Random.Range (0, randomMaterials_candy.Count);
		transform.GetChild(0).GetComponent<Renderer> ().material = randomMaterials_candy [randomNum];
		transform.GetChild(1).GetComponent<Renderer> ().material = randomMaterials_sides [randomNum];
		transform.GetChild(2).GetComponent<Renderer> ().material = randomMaterials_wrapper [randomNum];

	}

}
