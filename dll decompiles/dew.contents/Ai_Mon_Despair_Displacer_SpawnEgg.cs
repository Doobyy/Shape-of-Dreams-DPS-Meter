using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_Displacer_SpawnEgg : AbilityInstance
{
	public float delay;

	public float duration;

	public DewCollider range;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public KnockUpStrength knockUpStrength;

	public GameObject fxEgg;

	public GameObject fxTelegraph;

	public GameObject fxHit;

	public GameObject fxExplode;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			FxPlayNetworked(fxEgg);
			yield return new SI.WaitForSeconds(delay);
			FxPlayNetworked(fxTelegraph);
			yield return new SI.WaitForSeconds(duration);
			FxStopNetworked(fxEgg);
			FxPlayNetworked(fxExplode);
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				FxPlayNewNetworked(fxHit, entity);
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(((Component)(object)this).transform.position).Dispatch(entity);
				knockback.ApplyWithOrigin(((Component)(object)this).transform.position, entity);
				entity.Visual.KnockUp(knockUpStrength, isFriendly: false);
			}
			handle.Return();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
