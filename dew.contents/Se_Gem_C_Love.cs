using Mirror;
using UnityEngine;

public class Se_Gem_C_Love : StatusEffect
{
	public ScalingValue bonusAmount;

	public float duration;

	public float selfReduction;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			float num = (((Object)(object)info.caster == (Object)(object)victim) ? (1f - selfReduction) : 1f);
			DoHaste(GetValue(bonusAmount) * num);
			DoSpeed(GetValue(bonusAmount) * num);
			SetTimer(duration);
			ShowOnScreenTimer("Gem_C_Love");
		}
	}

	private void MirrorProcessed()
	{
	}
}
