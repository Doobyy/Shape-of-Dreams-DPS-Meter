using Mirror;
using UnityEngine;

public class Se_R_BackOff_Shielded : StatusEffect
{
	public ScalingValue armorAmount;

	public ScalingValue shieldAmount;

	public float buffDuration = 3f;

	public float casterAmp;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			float num = (((Object)(object)info.caster == (Object)(object)victim) ? (1f + casterAmp) : 1f);
			DoArmorBoost(GetValue(armorAmount) * num);
			DoShield(GetValue(shieldAmount) * num);
			SetTimer(buffDuration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
