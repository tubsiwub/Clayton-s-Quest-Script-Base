using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropPainter : MonoBehaviour
{
	[SerializeField] GameObject[] props = new GameObject[1];
	[SerializeField] bool randomYaw = true;
	[SerializeField] bool randomRotation = false;
	[SerializeField] float brushSize = 1;
	[SerializeField] float brushDensity = maxBrushDensity;
	const float maxBrushDensity = 30;

	public GameObject[] Props { get { return props; } }
	public bool RandomYaw { get { return randomYaw; } }
	public bool RandomRotation { get { return randomRotation; } }
	public float BrushSize { get { return brushSize; } }
	public float BrushDensity { get { return brushDensity; } }
	public float MaxBrushDensity { get { return maxBrushDensity; } }
}
