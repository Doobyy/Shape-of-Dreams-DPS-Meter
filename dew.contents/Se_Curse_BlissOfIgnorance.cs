using Mirror;
using UnityEngine;

public class Se_Curse_BlissOfIgnorance : CurseStatusEffect
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Status.HideHealth();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.Status.ShowHealth();
		}
	}

	private void MirrorProcessed()
	{
	}
}
