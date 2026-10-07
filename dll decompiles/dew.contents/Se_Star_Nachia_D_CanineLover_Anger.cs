using System;
using Mirror;

public class Se_Star_Nachia_D_CanineLover_Anger : StatusEffect
{
	public float duration = 3f;

	[NonSerialized]
	public float amount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoHaste(amount);
			SetTimer(duration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
