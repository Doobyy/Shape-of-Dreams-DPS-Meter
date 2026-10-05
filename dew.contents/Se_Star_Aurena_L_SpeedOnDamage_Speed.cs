using System;
using Mirror;

public class Se_Star_Aurena_L_SpeedOnDamage_Speed : StatusEffect
{
	public float duration = 3f;

	[NonSerialized]
	public float amount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSpeed(amount);
			SetTimer(duration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
