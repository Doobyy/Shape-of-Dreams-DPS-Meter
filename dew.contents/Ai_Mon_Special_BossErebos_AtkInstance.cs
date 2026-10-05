using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_AtkInstance : AbilityInstance
{
	public float castDelay;

	public float particleDelay;

	public ScalingValue dmgFactor;

	public DewCollider range;

	public Knockback Knockback;

	public GameObject fxInstance;

	public GameObject fxTelegraph;

	public GameObject fxHit;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxTelegraph, info.point, null);
			yield return new SI.WaitForSeconds(particleDelay);
			FxPlayNetworked(fxInstance, info.point + Vector3.up * 1.5f, null);
			yield return new SI.WaitForSeconds(castDelay - particleDelay);
			range.transform.position = info.point;
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				FxPlayNewNetworked(fxHit, entity);
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetElemental(ElementalType.Light).SetOriginPosition(info.point).Dispatch(entity);
				Knockback.ApplyWithOrigin(info.point, entity);
			}
			handle.Return();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
