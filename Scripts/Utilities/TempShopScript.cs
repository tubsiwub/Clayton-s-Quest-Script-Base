using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempShopScript : MonoBehaviour 
{

	GameObject shop;
    GameObject player;
    GameObject cam;
	GameObject eventSystem;
	public GameObject menuSelected;

	// Use this for initialization
	void Start ()
	{
		shop = GameObject.FindWithTag("Shop");
        player = GameObject.FindWithTag("Player");
        cam = GameObject.FindWithTag("MainCamera");
		eventSystem = GameObject.FindWithTag ("EventSystem");
    }
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player") 
		{
			shop.GetComponent<StoreManagerScript>().storeActive = true;
            player.GetComponent<PlayerHandler>().SetFrozen(true, true);
            cam.GetComponent<CameraControl>().enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
			eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject (menuSelected);
        } 
	}

	void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player") 
		{
			shop.GetComponent<StoreManagerScript>().storeActive = false;
		} 
	}

}


