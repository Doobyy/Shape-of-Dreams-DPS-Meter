using Mirror;
using UnityEngine;

public class Se_E_Rewind_Damage : StatusEffect
{
	public ScalingValue damage;

	public float damageDelay;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(damageDelay);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((Object)(object)victim == null))
		{
			Damage(damage).Dispatch(victim, chain);
		}
	}

	private void MirrorProcessed()
	{
	}
}
