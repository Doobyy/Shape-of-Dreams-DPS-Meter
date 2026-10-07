using Mirror;
using UnityEngine;

public class Se_Star_I_KillGoldBonus : StarEffect
{
	public StarScalingValue bonusRatio;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.owner.monsterKillGoldMultiplier += GetValue(bonusRatio);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.owner.monsterKillGoldMultiplier -= GetValue(bonusRatio);
		}
	}

	private void MirrorProcessed()
	{
	}
}
