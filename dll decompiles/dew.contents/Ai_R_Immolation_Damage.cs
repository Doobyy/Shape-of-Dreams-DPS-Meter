using Mirror;
using UnityEngine;

public class Ai_R_Immolation_Damage : TickDamageInstance
{
	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			((Component)(object)this).transform.position = info.caster.position;
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if ((Object)(object)info.caster != null)
		{
			((Component)(object)this).transform.position = info.caster.position;
		}
	}

	private void MirrorProcessed()
	{
	}
}
