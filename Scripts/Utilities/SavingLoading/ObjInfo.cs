using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjInfo : MonoBehaviour {

	[SerializeField] string prefabLocation;
	public bool assignRandomName = true;

	string objectRefID;

	public string GetRefID { get { return objectRefID; } }

	void Awake()
	{
		if (assignRandomName)
			name = prefabLocation + "_" + MathFunctions.GetHexStringFromPosition(transform.position);

		objectRefID = name;
	}

	void Start()
	{
		LOAD();
	}

	public void SAVE(bool overwrite, bool exists)
	{
		GameObject obj = GameObject.Find(objectRefID);

		if (obj != null)
		{
			SavingLoading.instance.SaveObject(
				objectRefID,
				SceneManager.GetActiveScene().buildIndex,
				prefabLocation,
				obj.transform.position,
				obj.transform.rotation,
				obj.transform.localScale,
				overwrite, exists);
		}
	}

	public void LOAD()
	{
		SavingLoading.StoredObject storedObj = SavingLoading.instance.LoadObject(objectRefID);

		if (storedObj.objectRef != SavingLoading.instance.nullObject.objectRef && storedObj.presentSceneIndex != -1)
		{
			GameObject obj = GameObject.Find(objectRefID);

			if (!storedObj.exists)
				Destroy(obj);

			obj.transform.position = new Vector3(
				storedObj.worldPositionX,
				storedObj.worldPositionY,
				storedObj.worldPositionZ);

			obj.transform.rotation = Quaternion.Euler(new Vector3(
				storedObj.worldRotationX,
				storedObj.worldRotationY,
				storedObj.worldRotationZ));

			obj.transform.localScale = new Vector3(
				storedObj.worldScaleX,
				storedObj.worldScaleY,
				storedObj.worldScaleZ);
		}
	}
}
