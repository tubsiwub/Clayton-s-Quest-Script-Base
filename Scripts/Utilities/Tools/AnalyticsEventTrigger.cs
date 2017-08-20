using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

[RequireComponent(typeof(SphereCollider))]
public class AnalyticsEventTrigger : MonoBehaviour
{
	[SerializeField] string customEventName;
	bool sentEvent = false;

	public void Init(string eventName, float sphereRadius)
	{
		SphereCollider col = GetComponent<SphereCollider>();
		col.isTrigger = true;

		customEventName = eventName;
		col.radius = sphereRadius;
	}

	void OnTriggerEnter(Collider obj)
	{
		if (obj.tag == "Player" && !sentEvent)
		{
			if (obj.GetComponent<PlayerHandler>().Ready)
			{
				print("Analytics sent " + customEventName + " event.");
				Analytics.CustomEvent(customEventName);
				sentEvent = true;
			}
		}
	}
}
