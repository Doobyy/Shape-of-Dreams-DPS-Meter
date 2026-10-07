using System;
using UnityEngine;

public class Ai_Shrine_CallOfTheRavenous_Projectile : StandardProjectile
{
	[NonSerialized]
	public SafeAction actionOnComplete;

	[NonSerialized]
	public Vector3 destination;

	protected override void OnPrepare()
	{
	}

	protected override void OnComplete()
	{
	}

	private void MirrorProcessed()
	{
	}
}
