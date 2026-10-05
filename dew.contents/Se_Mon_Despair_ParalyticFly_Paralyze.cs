using System.Collections;
using Mirror;
using UnityEngine;

public class Se_Mon_Despair_ParalyticFly_Paralyze : StatusEffect
{
	public int tickCount;

	public float slowAmount;

	public float tickInterval;

	public ScalingValue tickDamage;

	public GameObject tickHitEffect;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DoSlow(slowAmount);
			for (int i = 0; i < tickCount; i++)
			{
				yield return new SI.WaitForSeconds(tickInterval);
				DefaultDamage(tickDamage).SetAttr(DamageAttribute.DamageOverTime).Dispatch(victim);
				FxPlayNewNetworked(tickHitEffect, victim);
			}
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
