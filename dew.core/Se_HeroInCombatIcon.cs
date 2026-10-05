using System;
using Mirror;
using UnityEngine;

public class Se_HeroInCombatIcon : StatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (!(victim is Hero hero))
			{
				Destroy();
				return;
			}
			showIcon = hero.isInCombat;
			hero.ClientHeroEvent_OnIsInCombatChanged += new Action<bool>(ClientHeroEventOnIsInCombatChanged);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null && victim is Hero hero)
		{
			hero.ClientHeroEvent_OnIsInCombatChanged -= new Action<bool>(ClientHeroEventOnIsInCombatChanged);
		}
	}

	private void ClientHeroEventOnIsInCombatChanged(bool obj)
	{
		showIcon = obj;
	}

	private void MirrorProcessed()
	{
	}
}
