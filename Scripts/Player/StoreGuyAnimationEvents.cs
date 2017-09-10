using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreGuyAnimationEvents : HeroAnimationEvents
{
	private StorePreviewController storePreviewController;

	new protected void OnEnable()
	{
		storePreviewController = GetComponentInParent<StorePreviewController>();
	}

	public override void ToBall()
	{
		storePreviewController.EndToBall();
	}

	public override void ToHuman()
	{
		storePreviewController.EndToHuman();
	}
}
