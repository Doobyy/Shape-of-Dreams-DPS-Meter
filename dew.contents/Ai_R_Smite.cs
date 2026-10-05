using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_R_Smite : AbilityInstance, ACH_THEYRE_JUST_BIG_CATS.ILightingActor
{
	public float delay = 0.5f;

	public DewCollider range;

	public GameObject smiteEffect;

	public GameObject hitEffect;

	public ScalingValue damage;

	public float stunDuration = 2.5f;

	public float fireChance = 0.7f;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		((Component)(object)this).transform.position = info.point;
		yield return new SI.WaitForSeconds(delay);
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		FxPlayNetworked(smiteEffect);
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			Damage(damage).SetElemental(ElementalType.Light).Dispatch(entity);
			if (Random.value < fireChance)
			{
				ApplyElemental(ElementalType.Fire, entity);
			}
			FxPlayNewNetworked(hitEffect, entity);
			CreateBasicEffect(entity, new StunEffect(), stunDuration, "SmiteStun");
		}
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
