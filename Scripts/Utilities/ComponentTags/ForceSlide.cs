using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// put this on any object (with a collider) that you want the player to slide against, no matter what
public class ForceSlide : MonoBehaviour
{
	[Tooltip("Use this if you want to (attempt) to push the player off this object.")]
	[SerializeField] bool pushOff = false;
	
	public bool PushOff { get { return pushOff; } }
}
