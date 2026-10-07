using System;
using Mirror;
using UnityEngine;

public class Mon_DarkCave_SeekerHallucination : Monster, IForceHeroicHealthbar
{
	[NonSerialized]
	public float destroyHpThreshold;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			disableLoot = true;
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			if (normalizedHealth < destroyHpThreshold)
			{
				Kill();
			}
			else
			{
				AI.Helper_ChaseTarget();
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
