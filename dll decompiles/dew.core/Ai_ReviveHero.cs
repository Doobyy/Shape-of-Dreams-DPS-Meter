using System;
using Mirror;
using UnityEngine;

public class Ai_ReviveHero : AbilityInstance, IOtherPlayersTonedDownDisable
{
	[NonSerialized]
	public float reviveHealthMultiplier = -1f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)info.target != null)
		{
			if (info.target.Status.TryGetStatusEffect<Se_HeroKnockedOut>(out var effect))
			{
				effect.Revive(reviveHealthMultiplier);
			}
			CreateBasicEffect(info.target, new InvulnerableEffect(), 4f);
			CreateBasicEffect(info.target, new UntargetableEffect(), 2f);
			CreateBasicEffect(info.target, new InvisibleEffect
			{
				ignoreReveal = true
			}, 2f);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
