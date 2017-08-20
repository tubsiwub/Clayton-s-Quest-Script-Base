using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDFaceGuy : MonoBehaviour
{
	[SerializeField] Animator faceAnimator;
	string[] faces = new string[] { "Shocked", "Distraught", "Angry", "Frown", "Smile", "Overjoyed" };
	int currentFace = -1;

	// only get started by a call from health manager at scene load
	public void SetFace(int face)
	{
		if (face != currentFace)
		{
			faceAnimator.SetTrigger(faces[face]);
			currentFace = face;
		}
	}
}
