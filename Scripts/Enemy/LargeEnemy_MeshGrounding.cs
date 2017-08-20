using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LargeEnemy_MeshGrounding : MonoBehaviour {


	float height = 2.5f;

	void Start () {
		
	}

	void Update () {

		Ray ray = new Ray (transform.position + Vector3.up * 1, Vector3.down * 30);
		RaycastHit hit;
//		if (Physics.Raycast (ray, out hit)) {
//			if (hit.collider != this.GetComponent<BoxCollider> ())
//				transform.position = Vector3.Lerp(
//					transform.position,
//					new Vector3 (transform.position.x, hit.transform.position.y + (height), transform.position.z),
//					4*Time.deltaTime);
//		}

		if (Physics.BoxCast (transform.position + Vector3.up * 2, GetComponent<MeshRenderer> ().bounds.extents, Vector3.down * 30, out hit)) {
			if(hit.collider != this.GetComponent<BoxCollider> () && hit.transform.tag != "Player")
				transform.position = Vector3.Lerp(
					transform.position,
					new Vector3 (transform.position.x, hit.transform.position.y + (height), transform.position.z),
					4*Time.deltaTime);
		}

		Debug.DrawRay (ray.origin, ray.direction, Color.red);

	}
}
