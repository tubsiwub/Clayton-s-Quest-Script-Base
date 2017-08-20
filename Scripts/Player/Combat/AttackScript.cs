using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
    //temp for fake fisty hammers
    public GameObject[] hammyModels;

    public GameObject player;

    //Which attack in combo is it?
    int attackCount = 0;

	bool isAttacking = false;
	bool comboCooldown = false;
    bool comboPause = false;

	//timers
    float attackTimer = 0.0f;
    float attackReset = 0.6f;
	float animTimer = 0.0f;
	float animReset = 0.4f;
	float comboTimer = 0.0f;
	float comboReset = 1.2f;
    float comboPauseTimer = 0.0f;
    float comboPauseReset = 0.3f;

    ////original hammer spots
    Vector3 h1Pos;
    Vector3 h2Pos;
    Vector3 h3Pos;

	public Animator anim;

    void Start()
    {
		player = GameObject.FindWithTag("Player");

        //makes all hammer models invis
        for (int i = 0; i < hammyModels.Length; i++)
        {
            hammyModels[i].SetActive(false);
        }
    }

    void Update()
    {
		if (player.GetComponent<PlayerHandler>().CurrentState == PlayerHandler.PlayerState.Human)
		{	
			if (!anim.isInitialized)
				return;
		
			//timer stuff
			float elapsed = Time.deltaTime;
			attackTimer += elapsed;

			if (isAttacking) 
			{
				animTimer += elapsed;
			} 

			else 
			{
				animTimer = 0.0f;
			}

			if (comboCooldown)
				comboTimer += elapsed;

			if (comboTimer >= comboReset)
			{
				comboTimer = 0.0f;
				comboCooldown = false;
			}

			if (comboPause)
				comboPauseTimer += elapsed;

			if (comboPauseTimer >= comboPauseReset)
			{
				comboPauseTimer = 0.0f;
				comboPause = false;
			}


			if (animTimer >= animReset) 
			{
//			anim.SetBool ("Attack1", false);
//			anim.SetBool ("Attack2", false);
//			anim.SetBool ("Attack3", false);
				anim.SetBool ("NoAttack", true);
			}

			//Debug.Log (comboTimer);
			if (attackTimer >= attackReset)
			{
				//resets attack combo
				attackCount = 0;
				isAttacking = false;
		
				anim.SetBool ("Attack1", false);
				anim.SetBool ("Attack2", false);
				anim.SetBool ("Attack3", false);
				anim.SetBool ("NoAttack", true);

				//makes hammer models invis
				for (int i = 0; i < hammyModels.Length; i++) 
				{
					hammyModels [i].SetActive (false);
				}
	
				attackTimer = 0.0f;
				animTimer = 0.0f;
			}

			if (Input.GetButtonDown ("Attack")) {
				isAttacking = true;

				//attack1
				if (isAttacking && attackCount == 0 && !comboCooldown)
				{
					//keeps combo going without resetting

					anim.SetBool ("Attack1", true);
					anim.SetBool ("Attack2", false);
					anim.SetBool ("Attack3", false);
					anim.SetBool ("NoAttack", false);
					animTimer = 0.0f;

					attackTimer = 0.0f;
					isAttacking = true;
					attackCount = 1;
					comboPause = true;
				} 

            //attack 2
			else if (attackCount == 1 && isAttacking && attackTimer <= attackReset && !comboPause)
				{
					anim.SetBool ("Attack1", false);
					anim.SetBool ("Attack2", true);
					anim.SetBool ("Attack3", false);
					animTimer = 0.0f;


					attackTimer = 0.0f;
					isAttacking = true;
					attackCount = 2;
					comboPause = true;
				}

            //attack 3
			else if (attackCount == 2 && isAttacking && attackTimer <= attackReset && !comboPause)
				{
					anim.SetBool ("Attack1", false);
					anim.SetBool ("Attack2", false);
					anim.SetBool ("Attack3", true);
					animTimer = 0.0f;

					//Debug.Log ("YES!");
					//resets combo
					attackTimer = 0.0f;
					attackCount = 0;
					isAttacking = false;
					comboCooldown = true;
				}
			}
		}
    }

	//hitbox calls from animation events
	public void ShowAttack1()
	{
		hammyModels [2].SetActive (false);
		hammyModels [1].SetActive (false);
		hammyModels [0].SetActive (true);
	}

	public void HideAttack1()
	{
		hammyModels [2].SetActive (false);
		hammyModels [1].SetActive (false);
		hammyModels [0].SetActive (false);
	}
		

	public void ShowAttack2()
	{
		hammyModels [0].SetActive (false);
		hammyModels [2].SetActive (false);
		hammyModels [1].SetActive (true);
	}

	public void HideAttack2()
	{
		hammyModels [2].SetActive (false);
		hammyModels [1].SetActive (false);
		hammyModels [0].SetActive (false);
	}

	public void ShowAttack3()
	{
		hammyModels [1].SetActive (false);
		hammyModels [0].SetActive (false);
		hammyModels [2].SetActive (true);
	}

	public void HideAttack3()
	{
		hammyModels [2].SetActive (false);
		hammyModels [1].SetActive (false);
		hammyModels [0].SetActive (false);
	}
}