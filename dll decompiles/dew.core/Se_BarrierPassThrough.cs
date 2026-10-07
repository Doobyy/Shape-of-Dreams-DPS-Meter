using System;
using Mirror;
using UnityEngine;

public class Se_BarrierPassThrough : StatusEffect, IOtherPlayersTonedDownDisable
{
	[NonSerialized]
	public Vector3 destination;

	public float duration = 0.5f;

	public DewEase ease;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (victim is Hero)
			{
				DoInvulnerable();
				DoUncollidable();
				DoUntargetable();
			}
			victim.Control.StartDisplacement(new DispByDestination
			{
				destination = destination,
				canGoOverTerrain = true,
				duration = duration,
				ease = ease,
				isFriendly = true,
				rotateForward = true,
				isCanceledByCC = false,
				onFinish = DestroyIfActive,
				onCancel = DestroyIfActive
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
