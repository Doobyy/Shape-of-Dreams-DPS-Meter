using System;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_F_LG_RangeAndSpeed : StarEffect
{
	public float rangeAmp;

	public override Type heroType => typeof(Hero_Mist);

	public override Type skillType => typeof(St_Q_Lunge);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)skill != null)
		{
			skill.configs[0].castMethod._range *= 1f + rangeAmp;
			skill.SyncCastMethodChanges(0);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)skill != null)
		{
			skill.configs[0].castMethod._range /= 1f + rangeAmp;
			skill.SyncCastMethodChanges(0);
		}
	}

	private void MirrorProcessed()
	{
	}
}
