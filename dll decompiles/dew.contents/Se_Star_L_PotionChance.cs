using Mirror;
using UnityEngine;

public class Se_Star_L_PotionChance : StarEffect
{
	public StarScalingValue potionChanceIncrease;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.owner.potionDropChanceMultiplier *= GetValue(potionChanceIncrease);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.owner.potionDropChanceMultiplier /= GetValue(potionChanceIncrease);
		}
	}

	private void MirrorProcessed()
	{
	}
}
