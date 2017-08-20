using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreManagerScript : MonoBehaviour 
{
	int item = 1;
	int colorSwap = 0;

	public bool storeActive;
	bool whitePressed;
	bool bluePressed;

	//array of each item
	public GameObject[] items;

    GameObject player;
    GameObject store;
    GameObject cam;
//	GameObject eventSystem;
//	public GameObject menuSelected;

    public Button white;
	public Button blue;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.FindWithTag("Player");
		store = GameObject.Find ("Store");
        cam = GameObject.FindWithTag("MainCamera");
		//eventSystem = GameObject.FindWithTag ("EventSystem");
    }
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown("c"))
			storeActive = true;

		if(!whitePressed)
			white.image.color = Color.white;

		if(!bluePressed)
			blue.image.color = Color.white;

        //if in store, bring up store UI
		if (!storeActive)
		{
			store.SetActive (false);
		}

		if (storeActive)
		{
			store.SetActive (true);
		}

		if (storeActive && item == 1)
		{
			items [0].SetActive (true);
			items [1].SetActive (false);
			items [2].SetActive (false);
		}

		if (storeActive && item == 2)
		{
			items [0].SetActive (false);
			items [1].SetActive (true);
			items [2].SetActive (false);
		}

		if (storeActive && item == 3)
		{
			items [0].SetActive (false);
			items [1].SetActive (false);
			items [2].SetActive (true);
		}
	}

	public void ClickRight()
	{
		if (storeActive && item == 1)
		{
			item = 2;
			Debug.Log (item);
		}

		else if (storeActive && item == 2)
		{
			item = 3;
			Debug.Log (item);
		}

		else if (storeActive && item == 3)
		{
			item = 1;
			Debug.Log (item);
		}
	}

	public void ClickLeft()
	{
		if (storeActive && item == 1)
		{
			item = 3;
			Debug.Log (item);
		}

		else if (storeActive && item == 2)
		{
			item = 1;
			Debug.Log (item);
		}

		else if (storeActive && item == 3)
		{
			item = 2;
			Debug.Log (item);
		}
	}

	public void White()
	{
		whitePressed = !whitePressed;

		if (whitePressed)
		{
			bluePressed = false;
			white.image.color = Color.grey;
			Debug.Log ("White is selected");
		} 

		else 
		{
			white.image.color = Color.white;
			Debug.Log ("White is deselected");
		}

		colorSwap = 1;
	}

	public void Blue()
	{
		bluePressed = !bluePressed;

		if (bluePressed)
		{
			whitePressed = false;
			blue.image.color = Color.grey;
			Debug.Log ("Blue is selected");
		} 

		else 
		{
			blue.image.color = Color.white;
			Debug.Log ("Blue is deselected");
		}
		
		colorSwap = 2;
	}

	public void Cancel()
	{
		storeActive = false;
        player.GetComponent<PlayerHandler>().SetFrozen(false, true);
        Cursor.lockState = CursorLockMode.Locked;
        cam.GetComponent<CameraControl>().enabled = true;
        Cursor.visible = false;
    }

	public void Submit()
	{
		if (item == 1)
		{
			if (colorSwap == 1)
			{
				if (MarbleManager.instance.Collected >= 1) 
				{
					whitePressed = false;
					Debug.Log ("You are now White!");
					MarbleManager.instance.RemoveMarble();
				}

				else
					Debug.Log ("You can't afford this!");
			}

			if (colorSwap == 2)
			{
				if (MarbleManager.instance.Collected >= 1) 
				{
					bluePressed = false;
					Debug.Log ("You are now Blue!");
					MarbleManager.instance.RemoveMarble();
				}

				else
					Debug.Log ("You can't afford this!");
			}
		}

		//storeActive = false;
	}
}
