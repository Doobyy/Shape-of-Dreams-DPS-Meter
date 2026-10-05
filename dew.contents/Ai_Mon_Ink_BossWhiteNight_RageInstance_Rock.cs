using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNight_RageInstance_Rock : AbilityInstance
{
	public float castDuration;

	public GameObject fxTelegraph;

	public GameObject fxInstance;

	public GameObject fxHit;

	public DewCollider range;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNetworked(fxTelegraph, position, rotation);
			DestroyOnDeath(info.caster);
			yield return new SI.WaitForSeconds(castDuration);
			FxStopNetworked(fxTelegraph);
			range.transform.position = position;
			range.transform.rotation = rotation;
			FxPlayNetworked(fxInstance, position, rotation);
			yield return new SI.WaitForSeconds(0.05f);
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				FxPlayNewNetworked(fxHit, entity);
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(position).SetDirection(((Component)(object)this).transform.forward).Dispatch(entity);
				knockback.ApplyWithDirection(((Component)(object)this).transform.forward, entity);
			}
			handle.Return();
			yield return new SI.WaitForSeconds(0.1f);
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxTelegraph);
		}
	}

	private void MirrorProcessed()
	{
	}
}
