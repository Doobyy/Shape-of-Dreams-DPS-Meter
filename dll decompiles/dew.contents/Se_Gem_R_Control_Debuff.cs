using Mirror;
using UnityEngine;

public class Se_Gem_R_Control_Debuff : StatusEffect
{
	public float duration = 3f;

	public ScalingValue slowRatio;

	public float SlowAmount => (1f - 1f / (1f + Mathf.Max(0f, GetValue(slowRatio)))) * 100f;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSlow(SlowAmount);
			SetTimer(duration);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(startEffectVictim);
		}
	}

	private void MirrorProcessed()
	{
	}
}
