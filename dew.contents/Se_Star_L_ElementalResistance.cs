using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_L_ElementalResistance : StarEffect
{
	public StarScalingValue durationReduction;

	private readonly List<ElementalStatusEffect> _burnTargets = new List<ElementalStatusEffect>();

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ClientEntityEvent_OnStatusEffectAdded += new Action<EventInfoStatusEffect>(ClientEntityEventOnStatusEffectAdded);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)victim != null)
		{
			victim.ClientEntityEvent_OnStatusEffectAdded -= new Action<EventInfoStatusEffect>(ClientEntityEventOnStatusEffectAdded);
		}
		foreach (ElementalStatusEffect burnTarget in _burnTargets)
		{
			if ((UnityEngine.Object)(object)burnTarget != null)
			{
				burnTarget.dealtDamageProcessor.Remove(ReduceBurnDamage);
			}
		}
		_burnTargets.Clear();
	}

	private void ClientEntityEventOnStatusEffectAdded(EventInfoStatusEffect obj)
	{
		if (obj.effect is ElementalStatusEffect elementalStatusEffect)
		{
			if (obj.effect is Se_Elm_Fire)
			{
				elementalStatusEffect.dealtDamageProcessor.Add(ReduceBurnDamage);
				_burnTargets.Add(elementalStatusEffect);
			}
			else
			{
				elementalStatusEffect.decayTime *= 1f - GetValue(durationReduction);
			}
		}
	}

	private void ReduceBurnDamage(ref DamageData data, Actor actor, Entity target)
	{
		data.ApplyReduction(GetValue(durationReduction));
	}

	private void MirrorProcessed()
	{
	}
}
