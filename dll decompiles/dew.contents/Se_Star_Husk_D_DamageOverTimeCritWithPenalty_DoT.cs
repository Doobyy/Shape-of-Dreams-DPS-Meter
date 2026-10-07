using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_D_DamageOverTimeCritWithPenalty_DoT : StatusEffect
{
	[NonSerialized]
	public FinalDamageData sourceDamage;

	[NonSerialized]
	public float totalDamage;

	public int ticks = 5;

	public float tickInterval = 0.3f;

	public GameObject fxPerTick;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			for (int i = 0; i < ticks; i++)
			{
				yield return new SI.WaitForSeconds(tickInterval);
				DefaultDamage(totalDamage / (float)ticks, sourceDamage.procCoefficient / (float)ticks).SetSourceType(sourceDamage.type).SetElemental(sourceDamage.elemental).SetAmountOrigin(sourceDamage)
					.Dispatch(victim, chain);
				FxPlayNetworked(fxPerTick, victim);
			}
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
