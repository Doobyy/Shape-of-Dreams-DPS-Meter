using Mirror;
using UnityEngine;

public class Se_Mon_SnowMountain_BossSkoll_DeathFromAbove_Invulnerable : StatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoInvulnerable();
			DoUntargetable();
			DoUncollidable();
			DoInvisible(ignoreReveal: true);
			info.caster.Control.freeMovement = true;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((Object)(object)info.caster == null))
		{
			info.caster.Control.freeMovement = false;
		}
	}

	private void MirrorProcessed()
	{
	}
}
