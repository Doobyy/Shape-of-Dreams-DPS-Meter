using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_Meteor : AbilityInstance
{
	public float castDelay;

	public float maxHpDmgRatio;

	public ScalingValue dmgFactor;

	public DewCollider range;

	public GameObject fxTelegraph;

	public GameObject fxInstance;

	public GameObject fxEnd;

	public GameObject fxHit;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		FxPlayNetworked(fxTelegraph, info.point, Quaternion.identity);
		FxPlayNetworked(fxInstance, info.point, Quaternion.identity);
		yield return new SI.WaitForSeconds(castDelay - 0.25f);
		FxStopNetworked(fxInstance);
		yield return new SI.WaitForSeconds(0.25f);
		FxPlayNetworked(fxEnd, info.point, Quaternion.identity);
		range.transform.position = info.point;
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			FxPlayNewNetworked(fxHit, entity);
			CreateDamage(DamageData.SourceType.Default, GetValue(dmgFactor) + entity.maxHealth * maxHpDmgRatio).SetOriginPosition(info.point).SetElemental(ElementalType.Fire).Dispatch(entity);
			if (!entity.Status.hasCrowdControlImmunity)
			{
				entity.Visual.KnockUp(3f, isFriendly: false);
			}
		}
		handle.Return();
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxInstance);
			FxStopNetworked(fxTelegraph);
		}
	}

	private void MirrorProcessed()
	{
	}
}
