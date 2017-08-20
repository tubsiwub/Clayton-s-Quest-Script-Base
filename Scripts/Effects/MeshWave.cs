﻿// MeshWave.cs
// C#
using UnityEngine;
using System.Collections;

// Borrowed from online

public class MeshWave : MonoBehaviour
{
	[System.Serializable]
	public class WaveSystem
	{
		private Quaternion m_Rotation;
		private Vector3 m_Direction;
		public float length;
		public float speed ;
		public float scale ;
		public float Length { get { return length; } set{length = value; }}
		public float Speed { get { return speed; } set{speed = value; }}
		public float Scale { get { return scale; } set{scale = value; }}
		public Vector3 Direction
		{
			get {return m_Direction;}
			set
			{
				m_Direction = value.normalized;
				m_Rotation = Quaternion.Inverse(Quaternion.LookRotation(-m_Direction));
			}
		}

		public Vector3 ApplyHeight(Vector3 aPos)
		{
			Vector3 dir = m_Rotation * aPos;
			if (Length != 0)
				dir.z *= 1.0f/Length;
			aPos.y += Mathf.Sin(Time.time * Speed+ dir.z ) * Scale;
			return aPos;
		}

	}

	public Transform Target1;
	public Transform Target2;
	public WaveSystem[] waves;
	Vector3[] m_BaseHeight;
	Mesh m_WaterPlane;


	void CalculateWaves ()
	{
		waves[0].Direction = Target1.position - transform.position;
		waves[1].Direction = Target2.position - transform.position;

		if (m_BaseHeight == null)
			m_BaseHeight = m_WaterPlane.vertices;
		Vector3[] WaterVertices = new Vector3[m_BaseHeight.Length];
		for (int i=0; i< WaterVertices.Length; i++)
		{
			Vector3 Vertex = m_BaseHeight[i];
			foreach(var W in waves)
			{
				Vertex = W.ApplyHeight(Vertex);
			}
			WaterVertices[i] = Vertex;
		}
		m_WaterPlane.vertices = WaterVertices;
		m_WaterPlane.RecalculateNormals ();
	}
	void Start ()
	{
		m_WaterPlane = GetComponent<MeshFilter>().mesh;

	}

	void Update ()
	{
		CalculateWaves();

	}
}