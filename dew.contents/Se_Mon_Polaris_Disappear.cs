using Mirror;
using UnityEngine;

public class Se_Mon_Polaris_Disappear : StatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoUncollidable();
			DoInvulnerable();
			DoUntargetable();
			DoInvisible(ignoreReveal: true);
			victim.Visual.DisableRenderers();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.Visual.EnableRenderers();
		}
	}

	private void MirrorProcessed()
	{
	}
}
