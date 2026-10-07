using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_FireElemental_DestructSub : AbilityInstance
{
	public GameObject telegraph;

	public GameObject hitEffect;

	public GameObject explodeEffect;

	public GameObject deathEffect;

	public DewAnimationClip startAnim;

	public DewCollider range;

	public Knockback knockback;

	public float startAnimSpeed;

	public float explodeDuration = 10f;

	public float explodeDelay = 3.5f;

	public ScalingValue dmgFactor;

	public float knockupStrength = 1.5f;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		yield return base.OnCreateSequenced();
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		CreateBasicEffect(info.caster, new UnstoppableEffect(), float.PositiveInfinity);
		DestroyOnDeath(info.caster);
		yield return new SI.WaitForSeconds(0.1f);
		FxPlayNetworked(telegraph);
		info.caster.Animation.PlayAbilityAnimation(startAnim, startAnimSpeed);
		yield return new SI.WaitForSeconds(explodeDelay);
		info.caster.Visual.DisableRenderers();
		info.caster.Control.StartDaze(explodeDuration);
		FxPlayNetworked(explodeEffect);
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			CreateDamage(DamageData.SourceType.Default, dmgFactor).SetElemental(ElementalType.Fire).Dispatch(entity);
			knockback.ApplyWithOrigin(info.caster.position, entity);
			FxPlayNewNetworked(hitEffect, entity);
			if (!entity.Status.hasUnstoppable)
			{
				entity.Visual.KnockUp(knockupStrength, isFriendly: false);
			}
		}
		handle.Return();
		CreateAbilityInstance<Ai_Mon_LavaLand_FireElemental_ExplosionSub>(position, null, new CastInfo(info.caster));
		FxPlayNetworked(deathEffect, info.caster);
		info.caster.Kill();
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		((Component)(object)this).transform.position = Dew.GetPositionOnGround(info.caster.position);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(telegraph);
		}
	}

	private void MirrorProcessed()
	{
	}
}
