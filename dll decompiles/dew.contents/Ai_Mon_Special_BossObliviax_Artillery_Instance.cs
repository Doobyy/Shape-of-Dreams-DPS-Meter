using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_Artillery_Instance : AbilityInstance
{
	[HideInInspector]
	public float delay;

	public float dmgDelay;

	public ScalingValue dmgFactor;

	public DewCollider range;

	public Knockback knockback;

	public GameObject fxTelegraph;

	public GameObject fxInstance;

	public GameObject fxHit;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxTelegraph, info.point, null);
			yield return new SI.WaitForSeconds(delay);
			FxStopNetworked(fxTelegraph);
			FxPlayNetworked(fxInstance, info.point, null);
			yield return new SI.WaitForSeconds(dmgDelay);
			range.transform.position = info.point;
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.point).Dispatch(entity);
				CreateBasicEffect(entity, new SlowEffect
				{
					decay = true,
					strength = 50f
				}, 1f, "ObvArtillerySlow", DuplicateEffectBehavior.UsePrevious);
				FxPlayNewNetworked(fxHit, entity);
			}
			handle.Return();
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
