using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_Teleport_Instance : AbilityInstance
{
	public DewCollider range;

	public ScalingValue dmgFactor;

	public GameObject fxInstance;

	public GameObject fxHit;

	public Knockback knockback;

	public float enhancedAtkDelay;

	public DewCollider enhancedRange;

	public GameObject fxEnhanced;

	public GameObject fxEnhancedTelegraph;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		FxPlayNetworked(fxInstance, info.point, null);
		range.transform.position = info.point;
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			FxPlayNewNetworked(fxHit, entity);
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.point).Dispatch(entity);
			knockback.ApplyWithDirection((entity.agentPosition - info.point).normalized, entity);
		}
		handle.Return();
		if (!((Mon_Special_BossErebos)info.caster)._isPhaseChanged)
		{
			Destroy();
			yield break;
		}
		FxPlayNetworked(fxEnhancedTelegraph, info.point, null);
		yield return new SI.WaitForSeconds(enhancedAtkDelay);
		FxPlayNetworked(fxEnhanced, info.point, null);
		entities = enhancedRange.GetEntities(out var handle2, tvDefaultHarmfulEffectTargets);
		for (int j = 0; j < entities.Count; j++)
		{
			Entity entity2 = entities[j];
			FxPlayNewNetworked(fxHit, entity2);
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetDirection((entity2.agentPosition - info.point).normalized).Dispatch(entity2);
			knockback.ApplyWithDirection((entity2.agentPosition - info.point).normalized, entity2);
		}
		handle2.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
