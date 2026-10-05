using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Se_C_Hemorrhage_Bleed : StatusEffect
{
	public ScalingValue totalDamage;

	public ScalingValue ticks;

	public float ticksInterval = 0.25f;

	public float firstProcCoefficient = 1f;

	public float subsequentProcCoefficient = 0.35f;

	public float slowAmount = 70f;

	[NonSerialized]
	public float strength = 1f;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		strength = 1f;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DoSlow(slowAmount);
			int ticksValue = Mathf.RoundToInt(GetValue(ticks));
			for (int i = 0; i < ticksValue; i++)
			{
				yield return new SI.WaitForSeconds(ticksInterval);
				Damage(totalDamage, (i == 0) ? firstProcCoefficient : subsequentProcCoefficient).ApplyRawMultiplier(1f / (float)ticksValue).ApplyStrength(strength).Dispatch(victim, chain);
			}
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
