using System;
using System.Collections;
using Mirror;

public class Se_Mon_Special_BossMaw_ShadowWalk : StatusEffect
{
	[NonSerialized]
	public float disappearDuration = 0.25f;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DoUntargetable();
			DoUncollidable();
			SetTimer(disappearDuration);
		}
		yield break;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		_ = ((NetworkBehaviour)this).isServer;
	}

	private void MirrorProcessed()
	{
	}
}
