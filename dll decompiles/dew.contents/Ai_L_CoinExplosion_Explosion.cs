using System.Collections;
using Mirror;
using UnityEngine;

public class Ai_L_CoinExplosion_Explosion : InstantDamageInstance
{
	public GameObject fxSuccess;

	public GameObject fxFail;

	public float stunDuration;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		yield return base.OnCreateSequenced();
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		AbilityTrigger abilityTrigger = firstTrigger;
		if (abilityTrigger is St_L_CoinExplosion st)
		{
			yield return new SI.WaitForSeconds(Random.Range(0.25f, 0.4f));
			if (Random.value < 0.5f || ((double)Mathf.Abs(st.damageMultiplier - 1f) < 0.1 && Random.value < 0.4f))
			{
				FxPlayNetworked(fxSuccess, info.caster);
				st.NetworkdamageMultiplier = st.damageMultiplier * 2f;
			}
			else
			{
				FxPlayNetworked(fxFail, info.caster);
				st.NetworkdamageMultiplier = 1f;
			}
		}
		Destroy();
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (firstTrigger is St_L_CoinExplosion st_L_CoinExplosion)
		{
			float num = st_L_CoinExplosion.damageMultiplier - 1f;
			if (num > 0.01f)
			{
				dmg.SetAttr(DamageAttribute.IsCrit);
				dmg.ApplyAmplification(num);
			}
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration);
	}

	private void MirrorProcessed()
	{
	}
}
