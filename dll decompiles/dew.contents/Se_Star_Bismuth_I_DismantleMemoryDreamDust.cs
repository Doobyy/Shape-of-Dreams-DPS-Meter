using System;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_I_DismantleMemoryDreamDust : StarEffect
{
	public StarScalingValue dreamDustAmp;

	public override Type heroType => typeof(Hero_Bismuth);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			player.dismantleSkillDreamDustMultiplier *= 1f + GetValue(dreamDustAmp);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)player != null)
		{
			player.dismantleSkillDreamDustMultiplier /= 1f + GetValue(dreamDustAmp);
		}
	}

	private void MirrorProcessed()
	{
	}
}
