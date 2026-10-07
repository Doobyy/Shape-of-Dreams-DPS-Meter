using Mirror;
using UnityEngine;

public class Se_Curse_EternalLoop : CurseStatusEffect
{
	private ActorRef<Se_MirageSkin_Delusion_Delusional> _delusional;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_delusional = CreateStatusEffect(victim, (Se_MirageSkin_Delusion_Delusional se) =>
			{
				se.isEternal = true;
			});
			_delusional.Get().AddStack(9999);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((Object)(object)victim == null) && !_delusional.IsNullOrInactive())
		{
			_delusional.Get().Destroy();
			_delusional = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
