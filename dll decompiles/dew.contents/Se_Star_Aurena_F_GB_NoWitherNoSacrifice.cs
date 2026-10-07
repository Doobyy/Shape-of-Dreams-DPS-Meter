using System;
using Mirror;

public class Se_Star_Aurena_F_GB_NoWitherNoSacrifice : StarEffect
{
	public override Type heroType => typeof(Hero_Aurena);

	public override Type skillType => typeof(St_Q_GoldenBurst);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && skill is St_Q_GoldenBurst st_Q_GoldenBurst)
		{
			st_Q_GoldenBurst.NetworkcastForFreeIfNoWitherInCombat = true;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && skill is St_Q_GoldenBurst st_Q_GoldenBurst)
		{
			st_Q_GoldenBurst.NetworkcastForFreeIfNoWitherInCombat = false;
		}
	}

	private void MirrorProcessed()
	{
	}
}
