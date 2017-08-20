using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LOD_Control : MonoBehaviour
{
    public float[] distanceRanges;  // set # of distances = ammount of models
    public GameObject[] lodModels;  //asssign 3d meshes from highest quality to lowest quality

    private int current;

    private int level;

    // Use this for initialization
    void Start()
    {
        current = -2;
        level = -1;

        for (int i = 0; i < lodModels.Length; i++)
        {
            lodModels[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //compare camera distance to object
        float d = Vector3.Distance(Camera.main.transform.position, transform.position);

        // change level based on distance Ranges
        for (int i = 0; i < distanceRanges.Length; i++)
        {
            if (d < distanceRanges[i])
            {
                level = i;
                i = distanceRanges.Length;
            }
        }

        //added culling for furthest distance check
		if (d > distanceRanges [distanceRanges.Length - 1]) 
		{
			for (int j = 0; j < lodModels.Length; j++) {
				lodModels [j].SetActive (false);
			}
		} 
		else 
		{
			for (int j = 0; j < lodModels.Length; j++) {
				lodModels [j].SetActive (true);
			}
		}



       //check temp level variable
        if (level == -1)
        {
            level = distanceRanges.Length;
        }

        // chnage level of detail if not equal to current level
        if (current != level)
        {
            ChangeLOD(level);
        }
    }

    //function to change level of detail
    void ChangeLOD(int level)
    {
        lodModels[level].SetActive(true);

        if (current >= 0)
        {
            lodModels[current].SetActive(false);
        }

        current = level;
    }
}


