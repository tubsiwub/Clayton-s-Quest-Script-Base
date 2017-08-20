using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaplerScript : MonoBehaviour
{
	public GameObject staple;

	Animator anim;

    bool staplerOpen = false;
	bool firedStaple = false;

    float stapleSpeed = 10.0f;
    public float detectionRange;


    void Start ()
	{

        anim = transform.parent.transform.parent.GetComponent<Animator>();

    }
	
	void Update ()
	{

        LookForPlayer();

        FireStaple();

	}

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawRay (transform.position + new Vector3(0,0.5f,0), transform.forward * detectionRange);
		Gizmos.DrawRay (transform.position + new Vector3(0,-0.5f,0), transform.forward * detectionRange);
		Gizmos.DrawRay (transform.position + new Vector3(0.5f,0,0), transform.forward * detectionRange);
		Gizmos.DrawRay (transform.position + new Vector3(-0.5f,0,0), transform.forward * detectionRange);
		Gizmos.DrawRay (transform.position + new Vector3(0,0,0.5f), transform.forward * detectionRange);
		Gizmos.DrawRay (transform.position + new Vector3(0,0,-0.5f), transform.forward * detectionRange);
	}

    void LookForPlayer()
    {
        
        RaycastHit hit = new RaycastHit();

		if (Physics.BoxCast(transform.position, Vector3.one, transform.forward * detectionRange, out hit, Quaternion.identity, detectionRange, -1, QueryTriggerInteraction.Ignore))
        { 
            if(hit.collider.tag == "Player")
            {

                if (!staplerOpen && anim.GetCurrentAnimatorStateInfo(0).IsName("Stapler_closeIdle"))
                {
                    staplerOpen = true;
                    anim.SetTrigger("Open");
                }
            }

			else
			{
				if (staplerOpen && anim.GetCurrentAnimatorStateInfo(0).IsName("Stapler_openIdle"))
				{
					staplerOpen = false;
					anim.SetTrigger("Close");
				}
			}
        }

        else
        {
            if (staplerOpen && anim.GetCurrentAnimatorStateInfo(0).IsName("Stapler_openIdle"))
            {
                staplerOpen = false;
                anim.SetTrigger("Close");
            }
        }

    }
    
    void FireStaple()
    {

        if(staplerOpen && anim.GetCurrentAnimatorStateInfo(0).IsName("Stapler_openIdle"))
        {

            if (!firedStaple)
            {
                StartCoroutine(Stapling(1.0f));
            }

        }

    }

    IEnumerator Stapling(float delay)
    {

        firedStaple = true;

        GameObject stapleObj = Instantiate<GameObject>(staple, transform.position, Quaternion.identity);
        stapleObj.transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        stapleObj.transform.Rotate(270, 0, 0);
        stapleObj.transform.SetParent(this.transform);

        StartCoroutine(MoveStaple(stapleObj));

        yield return new WaitForSeconds(delay);

        firedStaple = false;

    }

    IEnumerator MoveStaple(GameObject stapleObj)
    {
        float despawnCounter = 5.0f;

        while (despawnCounter > 0)
        {
            despawnCounter -= Time.deltaTime;
            stapleObj.transform.position += transform.forward * stapleSpeed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Destroy(stapleObj);

        yield return new WaitForEndOfFrame();

    }

}
