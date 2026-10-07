using Mirror;

public class Se_Mon_LavaLand_BossInfernus_Jump_Invul : StatusEffect
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
			victim.Control.freeMovement = true;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Control.freeMovement = false;
		}
	}

	private void MirrorProcessed()
	{
	}
}
