using Mirror;
using UnityEngine;

public class Se_Star_L_ReceivedHealingAndShieldBonus : StarEffect
{
	public StarScalingValue ampAmount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.takenHealProcessor.Add(Processor);
			victim.takenShieldProcessor.Add(Processor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.takenHealProcessor.Remove(Processor);
			victim.takenShieldProcessor.Remove(Processor);
		}
	}

	private void Processor(ref HealData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyAmplification(GetValue(ampAmount));
		}
	}

	private void MirrorProcessed()
	{
	}
}
