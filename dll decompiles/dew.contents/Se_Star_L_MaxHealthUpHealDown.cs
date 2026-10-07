using Mirror;
using UnityEngine;

public class Se_Star_L_MaxHealthUpHealDown : StarEffect
{
	public StarScalingValue maxHealthAmp;

	public float healReduction;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoStatBonus(new StatBonus
			{
				maxHealthPercentage = GetValue(maxHealthAmp) * 100f
			});
			victim.takenHealProcessor.Add(TakenHealProcessor);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.takenHealProcessor.Remove(TakenHealProcessor);
		}
	}

	private void TakenHealProcessor(ref HealData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyReduction(healReduction);
		}
	}

	private void MirrorProcessed()
	{
	}
}
