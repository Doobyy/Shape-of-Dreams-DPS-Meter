using System;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_L_FireResistance : StarEffect
{
	public StarScalingValue fireDamageReduction;

	public StarScalingValue burnDamageReduction;

	public override Type heroType => typeof(Hero_Vesper);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.takenDamageProcessor.Add(Processor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.takenDamageProcessor.Remove(Processor);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			if (actor is Se_Elm_Fire)
			{
				data.ApplyReduction(GetValue(burnDamageReduction));
			}
			else if (data.elemental == ElementalType.Fire)
			{
				data.ApplyReduction(GetValue(fireDamageReduction));
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
