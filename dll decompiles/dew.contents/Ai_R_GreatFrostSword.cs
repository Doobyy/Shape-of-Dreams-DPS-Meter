using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_R_GreatFrostSword : AbilityInstance
{
	public ScalingValue dmgFactor;

	public Knockback knockback;

	public DewCollider range;

	public float singleTargetAmp = 1.5f;

	public GameObject fxSingleTargetAmp;

	public GameObject swordEffect;

	public GameObject swingEffect;

	public GameObject hitEffect;

	public DewAnimationClip startAnim;

	public DewAnimationClip endAnim;

	public float castingDuration;

	public float postDelay;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		info.caster.Control.StartDaze(postDelay + castingDuration);
		info.caster.Animation.PlayAbilityAnimation(startAnim);
		FxPlayNetworked(swordEffect, info.caster);
		yield return new SI.WaitForSeconds(castingDuration);
		info.caster.Animation.PlayAbilityAnimation(endAnim);
		FxPlayNetworked(swingEffect);
		yield return new SI.WaitForSeconds(0.1f);
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			FxPlayNewNetworked(hitEffect, entity);
			DamageData damageData = Damage(dmgFactor).SetElemental(ElementalType.Cold);
			if (entities.Count == 1)
			{
				damageData.ApplyAmplification(singleTargetAmp);
				damageData.SetAttr(DamageAttribute.IsCrit);
				FxPlayNewNetworked(fxSingleTargetAmp, entity);
			}
			damageData.Dispatch(entity);
			knockback.ApplyWithDirection(info.forward, entity);
		}
		handle.Return();
		FxStopNetworked(swordEffect);
		yield return new SI.WaitForSeconds(postDelay);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
