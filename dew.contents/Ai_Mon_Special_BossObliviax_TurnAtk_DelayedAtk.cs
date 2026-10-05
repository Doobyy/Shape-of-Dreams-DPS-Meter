using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossObliviax_TurnAtk_DelayedAtk : AbilityInstance
{
	public DewCollider range;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public float delay;

	public GameObject fxTelegraph;

	public GameObject fxExplode;

	public GameObject fxHit;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			FxPlayNetworked(fxTelegraph);
			yield return new SI.WaitForSeconds(delay);
			FxPlayNetworked(fxExplode);
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity entity = entities[i];
				FxPlayNewNetworked(fxHit, entity);
				knockback.ApplyWithOrigin(info.point, entity);
				entity.Visual.KnockUp(KnockUpStrength.Big, isFriendly: false);
				DefaultDamage(dmgFactor).SetOriginPosition(position).Dispatch(entity);
				CreateBasicEffect(entity, new SlowEffect
				{
					decay = true,
					strength = 80f
				}, 2.5f, "TurnAtkSlow", DuplicateEffectBehavior.UsePrevious);
			}
			handle.Return();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
