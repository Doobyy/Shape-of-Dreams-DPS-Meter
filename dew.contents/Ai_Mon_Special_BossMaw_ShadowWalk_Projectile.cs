using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_ShadowWalk_Projectile : AbilityInstance
{
	public float range;

	public float delay;

	public float dmgDelay;

	public Knockback knockback;

	public ScalingValue dmgFactor;

	public GameObject fxTelegraph;

	public GameObject fxInstance;

	public GameObject fxHit;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxTelegraph, info.point, Quaternion.identity);
			yield return new SI.WaitForSeconds(delay);
			FxPlayNetworked(fxInstance, info.point, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
			yield return new SI.WaitForSeconds(dmgDelay);
			List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.point, range, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < list.Count; i++)
			{
				Entity entity = list[i];
				CreateDamage(DamageData.SourceType.Physical, dmgFactor).SetElemental(ElementalType.Dark).SetOriginPosition(info.point).Dispatch(entity);
				FxPlayNewNetworked(fxHit, entity);
			}
			handle.Return();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
