using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossWhiteNight_BuddhasPalm_Instance : AbilityInstance
{
	[NonSerialized]
	public float startDelay;

	public GameObject fxInstance;

	public GameObject fxHit;

	public DewCollider range;

	public ScalingValue dmgFactor;

	public KnockUpStrength knockUpStrength;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			startDelay += 0.1f;
			info.caster.Control.StartDaze(startDelay);
			yield return new SI.WaitForSeconds(startDelay);
			FxPlayNetworked(fxInstance, ((Component)(object)this).transform.position, ((Component)(object)this).transform.rotation);
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetOriginPosition(info.point).Dispatch(entity);
				entity.Visual.KnockUp(knockUpStrength, isFriendly: false);
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
