using System;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_L_HealAmpOnLowHp : HealthThresholdBonusStarEffect
{
	public StarScalingValue healAmp;

	public override Type heroType => typeof(Hero_Aurena);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.dealtHealProcessor.Add(Processor);
			hero.takenHealProcessor.Add(Processor);
		}
	}

	private void Processor(ref HealData data, Actor actor, Entity target)
	{
		if (isLowHealth && !data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.SetCrit();
			data.ApplyAmplification(GetValue(healAmp));
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.dealtHealProcessor.Remove(Processor);
			hero.takenHealProcessor.Remove(Processor);
		}
	}

	private void MirrorProcessed()
	{
	}
}
