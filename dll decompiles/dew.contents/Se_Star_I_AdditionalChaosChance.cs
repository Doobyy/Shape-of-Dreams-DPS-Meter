using Mirror;
using UnityEngine;

public class Se_Star_I_AdditionalChaosChance : StarEffect
{
	public StarScalingValue doubleChaosChance;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.owner.doubleChaosChance += GetValue(doubleChaosChance);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.owner.doubleChaosChance -= GetValue(doubleChaosChance);
		}
	}

	private void MirrorProcessed()
	{
	}
}
