using System;
using Mirror;

public class Se_R_Parry_End : StatusEffect
{
	public float duration;

	[NonSerialized]
	public float blockedDamage;

	private float _lastEffectTime;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(duration);
			DoInvulnerable();
		}
	}

	private void MirrorProcessed()
	{
	}
}
