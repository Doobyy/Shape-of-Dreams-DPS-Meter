using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_BossDarkMoon_Eclipse_Instance : AbilityInstance
{
	public float dazeBeforStart;

	public float startDelay;

	public float postDelay;

	public float stunDuration;

	public DewCollider range;

	public ScalingValue dmgFactor;

	public Knockback knockback;

	public DewAnimationClip startClip;

	public DewAnimationClip loopClip;

	public DewAnimationClip endClip;

	public GameObject fxTelegraph;

	public GameObject fxAtk;

	public GameObject fxHit;

	public GameObject fxEnd;

	internal float _atkTime;

	protected override IEnumerator OnCreateSequenced()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			info.caster.Visual.EnableRenderers();
			yield return new SI.WaitForSeconds(dazeBeforStart);
			FxApplySpeedMultiplierNetworked(fxTelegraph, 1f / (_atkTime - Time.time));
			FxPlayNetworked(fxTelegraph, info.caster);
			info.caster.Animation.PlayAbilityAnimation(startClip);
			yield return new SI.WaitForSeconds(startDelay);
			info.caster.Animation.PlayAbilityAnimation(loopClip);
			yield return new SI.WaitForCondition(() => _atkTime - Time.time < 0.001f);
			FxPlayNetworked(fxAtk, info.caster);
			info.caster.Animation.PlayAbilityAnimation(endClip);
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int num = 0; num < entities.Count; num++)
			{
				Entity entity = entities[num];
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetDirection(((Component)(object)info.caster).transform.forward).SetElemental(ElementalType.Dark).Dispatch(entity);
				knockback.ApplyWithDirection(((Component)(object)info.caster).transform.forward, entity);
				FxPlayNewNetworked(fxHit, entity);
				CreateBasicEffect(entity, new StunEffect(), stunDuration, "eclipse_stun");
			}
			handle.Return();
			FxPlayNetworked(fxEnd, info.caster);
			yield return new SI.WaitForSeconds(postDelay);
			info.caster.Visual.DisableRenderers();
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
