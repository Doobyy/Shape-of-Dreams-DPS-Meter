using System;
using Mirror;

public class Se_Star_Bismuth_D_Conquerer : StarEffect
{
	public StarScalingValue statPerStack;

	private ActorRef<Se_Star_Bismuth_D_Conquerer_Stacks> _stacks;

	public override Type heroType => typeof(Hero_Bismuth);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_stacks = CreateStatusEffect(victim, new CastInfo(victim), (Se_Star_Bismuth_D_Conquerer_Stacks se) =>
			{
				se.apPerStack = GetValue(statPerStack);
				se.adPerStack = GetValue(statPerStack);
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !_stacks.IsNullOrInactive())
		{
			_stacks.Get().Destroy();
			_stacks = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
