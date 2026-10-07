using System;
using Mirror;

public class Se_Star_Cetus_L_ArmorBuffOnShield_Buff : StatusEffect
{
	public float duration = 2f;

	[NonSerialized]
	public int armorAmount;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(duration);
			DoArmorBoost(armorAmount);
			ShowOnScreenTimer("Se_Star_Cetus_L_ArmorBuffOnShield");
		}
	}

	private void MirrorProcessed()
	{
	}
}
