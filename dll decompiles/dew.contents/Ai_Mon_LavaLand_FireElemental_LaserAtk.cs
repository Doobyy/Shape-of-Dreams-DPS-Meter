using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_LavaLand_FireElemental_LaserAtk : AbilityInstance
{
	public DewCollider range;

	public DewCollider explosionRange;

	public ScalingValue dmgFactor;

	public GameObject hitEffect;

	public GameObject explosionEffect;

	public GameObject explosionTelegraph;

	public float slowStrength;

	public float slowDuration;

	public float explosionDelay;

	public Knockback explosionKnockback;

	private bool _isExplosion;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		yield return base.OnCreateSequenced();
		if (((NetworkBehaviour)this).isServer)
		{
			info.caster.Control.StartDaze(1f);
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Count; i++)
			{
				Entity e = entities[i];
				OnHit(e);
			}
			handle.Return();
			FxPlayNetworked(explosionTelegraph);
			yield return new SI.WaitForSeconds(explosionDelay);
			List<Entity> entities2 = explosionRange.GetEntities(out var handle2, tvDefaultHarmfulEffectTargets);
			_isExplosion = true;
			FxPlayNetworked(explosionEffect);
			for (int j = 0; j < entities2.Count; j++)
			{
				Entity e2 = entities2[j];
				OnHit(e2);
			}
			handle2.Return();
			Destroy();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_isExplosion = false;
	}

	private void OnHit(Entity e)
	{
		CreateDamage(DamageData.SourceType.Default, dmgFactor).SetElemental(ElementalType.Fire).Dispatch(e);
		CreateBasicEffect(e, new SlowEffect
		{
			strength = slowStrength
		}, slowDuration);
		if (_isExplosion)
		{
			explosionKnockback.ApplyWithOrigin(explosionEffect.transform.position, e);
		}
		FxPlayNewNetworked(hitEffect, e);
	}

	private void MirrorProcessed()
	{
	}
}
