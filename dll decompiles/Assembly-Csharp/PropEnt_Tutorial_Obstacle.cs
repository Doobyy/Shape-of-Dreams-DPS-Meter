using System;
using Mirror;

public class PropEnt_Tutorial_Obstacle : PropEntity, IHideHealthbar, IDisableGamepadTargeting, IDisableOcclusionTest
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			ClientEntityEvent_OnStatusEffectAdded += new Action<EventInfoStatusEffect>(ClientEntityEventOnStatusEffectAdded);
		}
	}

	private void ClientEntityEventOnStatusEffectAdded(EventInfoStatusEffect obj)
	{
		if (obj.effect is ElementalStatusEffect)
		{
			obj.effect.StopTimer();
			obj.effect.Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			ClientEntityEvent_OnStatusEffectAdded -= new Action<EventInfoStatusEffect>(ClientEntityEventOnStatusEffectAdded);
		}
	}

	private void MirrorProcessed()
	{
	}
}
