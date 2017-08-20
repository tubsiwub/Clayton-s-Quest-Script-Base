using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwirlTrackContainer : TrackTwirlBase
{
	[SerializeField] Platform_TrackTwirl trackTwirl;

	public override Platform_TrackTwirl Get()
	{
		return trackTwirl;
	}
}
