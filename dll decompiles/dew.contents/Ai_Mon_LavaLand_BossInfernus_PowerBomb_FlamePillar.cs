using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_BossInfernus_PowerBomb_FlamePillar : AbilityInstance
{
	public float initDelay;

	public GameObject fxTelegraph;

	public GameObject fxPillar;

	public GameObject fxHit;

	public float range;

	public int tickCount;

	public float tickInterval;

	public float tickDmgProcCoefficient;

	public ScalingValue firstDmgFactor;

	public ScalingValue tickDmgFactor;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		FxPlayNetworked(fxTelegraph);
		yield return new SI.WaitForSeconds(initDelay);
		FxPlayNetworked(fxPillar);
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, position, range, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < list.Count; i++)
		{
			Entity entity = list[i];
			CreateDamage(DamageData.SourceType.Default, firstDmgFactor).SetOriginPosition(position).SetElemental(ElementalType.Fire).Dispatch(entity);
			entity.Visual.KnockUp(KnockUpStrength.Big, isFriendly: false);
			FxPlayNewNetworked(fxHit, entity);
		}
		handle.Return();
		yield return null;
		for (int j = 0; j < tickCount; j++)
		{
			List<Entity> list2 = DewPhysics.OverlapCircleAllEntities(out var handle2, position, range, tvDefaultHarmfulEffectTargets);
			for (int k = 0; k < list2.Count; k++)
			{
				Entity victim = list2[k];
				CreateDamage(DamageData.SourceType.Default, tickDmgFactor, tickDmgProcCoefficient).SetOriginPosition(position).SetElemental(ElementalType.Fire).Dispatch(victim);
			}
			handle2.Return();
			yield return new SI.WaitForSeconds(tickInterval);
		}
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxPillar);
		}
	}

	private void MirrorProcessed()
	{
	}
}
