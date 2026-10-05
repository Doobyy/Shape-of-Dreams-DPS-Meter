using System;
using Mirror;
using UnityEngine;

public class Se_RoomMod_BlackRain : StatusEffect
{
	public float ampNonElementalDmg;

	public float elementalDmgReduction;

	public float elementalDurationReduction;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.takenDamageProcessor.Add(Processor);
			victim.ClientEntityEvent_OnStatusEffectAdded += new Action<EventInfoStatusEffect>(OnStatusEffectAdded);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !victim.IsNullInactiveDeadOrKnockedOut())
		{
			victim.takenDamageProcessor.Remove(Processor);
			victim.ClientEntityEvent_OnStatusEffectAdded -= new Action<EventInfoStatusEffect>(OnStatusEffectAdded);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (!victim.IsNullInactiveDeadOrKnockedOut() && !data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			if (data.elemental.HasValue)
			{
				data.ApplyReduction(elementalDmgReduction);
			}
			else if (!(actor is Se_HealthCost) && !((UnityEngine.Object)(object)actor.firstEntity == (UnityEngine.Object)(object)victim))
			{
				data.ApplyAmplification(ampNonElementalDmg);
			}
		}
	}

	private void OnStatusEffectAdded(EventInfoStatusEffect obj)
	{
		if (obj.effect is ElementalStatusEffect elementalStatusEffect)
		{
			elementalStatusEffect.decayTime *= 1f - elementalDurationReduction;
		}
	}

	private void MirrorProcessed()
	{
	}
}
