using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_SnowMountain_BossSkoll_AuraBlade : AbilityInstance
{
	public DewCollider range;

	public ScalingValue dmgFactor;

	public float delay;

	public GameObject cutEffect;

	public GameObject hitEffect;

	public KnockUpStrength knockUpStrength;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			yield return new SI.WaitForSeconds(delay);
			FxPlayNewNetworked(cutEffect);
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				entity.Visual.KnockUp(knockUpStrength, isFriendly: true);
				CreateDamage(DamageData.SourceType.Default, dmgFactor).Dispatch(entity);
				FxPlayNewNetworked(hitEffect, entity);
			}
			handle.Return();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
