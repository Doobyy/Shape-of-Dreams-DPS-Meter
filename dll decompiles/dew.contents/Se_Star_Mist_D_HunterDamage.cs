using System;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_D_HunterDamage : StarEffect
{
	public StarScalingValue damageAmp;

	public override Type heroType => typeof(Hero_Mist);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.dealtDamageProcessor.Add(Processor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.dealtDamageProcessor.Remove(Processor);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (target is Monster { isHunter: not false } && !data.IsAmountModifiedBy(this))
		{
			data.ApplyAmplification(GetValue(damageAmp));
			data.SetAmountModifiedBy(this);
		}
	}

	private void MirrorProcessed()
	{
	}
}
