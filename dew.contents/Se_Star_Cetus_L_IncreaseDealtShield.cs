using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_L_IncreaseDealtShield : StarEffect
{
	public StarScalingValue shieldBonusAmp;

	public override Type heroType => typeof(Hero_Cetus);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.dealtShieldProcessor.Add(Process);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.dealtShieldProcessor.Remove(Process);
		}
	}

	private void Process(ref HealData data, Actor from, Entity to)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.ApplyAmplification(GetValue(shieldBonusAmp));
			data.SetAmountModifiedBy(this);
		}
	}

	private void MirrorProcessed()
	{
	}
}
