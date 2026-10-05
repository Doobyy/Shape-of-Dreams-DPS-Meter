using Mirror;
using UnityEngine;

public class Se_Star_L_PotionHealAmount : StarEffect
{
	public StarScalingValue ampAmount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.takenHealProcessor.Add(Processor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.takenHealProcessor.Remove(Processor);
		}
	}

	private void Processor(ref HealData data, Actor actor, Entity target)
	{
		if (actor is Se_GenericHealOverTime && actor.parentActor is Ai_RegenOrb_Projectile && !data.IsAmountModifiedBy(this))
		{
			data.ApplyAmplification(GetValue(ampAmount));
			data.SetCrit();
			data.SetAmountModifiedBy(this);
		}
	}

	private void MirrorProcessed()
	{
	}
}
