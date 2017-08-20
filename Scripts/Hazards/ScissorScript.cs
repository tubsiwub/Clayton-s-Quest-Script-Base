using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScissorScript : MonoBehaviour 
{  
	bool isOpen = true;

	float scissorTimer = 0.0f;
	public float scissorDelay;

    Animator anim;

	void Start () 
	{

        anim = GetComponent<Animator>();

    }

	void Update ()
	{
		if (!isOpen)
		{		
			scissorTimer += Time.deltaTime;

			if (scissorTimer >= 0.1f)
			{
				isOpen = true;
				scissorTimer = 0.0f;
                anim.SetTrigger("Open");
            }
		}

		else
		{
			
			scissorTimer += Time.deltaTime;

			if (scissorTimer >= scissorDelay)
			{
				isOpen = false;
				scissorTimer = 0.0f;
                anim.SetTrigger("Close");
            }
        }
	}
}
