using System;
using UnityEngine;

[Serializable]
public struct AnimationClipWithSpeed
{
	public static AnimationClipWithSpeed Default = new AnimationClipWithSpeed
	{
		speed = 1f
	};

	public AnimationClip clip;

	public float speed;
}
