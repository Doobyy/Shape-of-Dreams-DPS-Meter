using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_GhostBlade_SwiftStep_Atk : AbilityInstance
{
	public DewAnimationClip startAnimation;

	public DewAnimationClip endAnimation;

	public float castDuration;

	public float postDelay;

	public ScalingValue dmgFactor;

	public DewCollider range;

	public GameObject fxSlash;

	public GameObject fxHit;

	public GameObject fxTelegraph;

	public Knockback Knockback;

	public float unequipStartTime;

	public float unequipEndTime;

	public GameObject fxUnequipStart;

	public GameObject fxUnequipEnd;

	[NonSerialized]
	public int addedProjectiles;

	private float _speedMultiplier = 1f;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		addedProjectiles = 0;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if ((UnityEngine.Object)(object)info.caster == null)
		{
			yield break;
		}
		_speedMultiplier = info.caster.Status.attackSpeedMultiplier;
		FxApplySpeedMultiplier(fxTelegraph, _speedMultiplier);
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			info.caster.Control.StartDaze(castDuration + postDelay / _speedMultiplier);
			info.caster.Animation.PlayAbilityAnimation(startAnimation, _speedMultiplier);
			FxPlayNetworked(fxTelegraph, info.caster);
			yield return new SI.WaitForSeconds(castDuration / _speedMultiplier);
			info.caster.Animation.StopAbilityAnimation(startAnimation);
			info.caster.Animation.PlayAbilityAnimation(endAnimation);
			FxPlayNetworked(fxSlash, info.caster);
			int num = addedProjectiles + 1;
			float num2 = 20f;
			for (int i = 0; i < num; i++)
			{
				float angle = info.angle - num2 * (float)(num - 1) * 0.5f + num2 * (float)i;
				CreateAbilityInstance<Ai_Mon_Ink_GhostBlade_SwiftStep_Projectile>(info.caster.agentPosition, null, new CastInfo(info.caster, angle));
			}
			range.transform.position = info.caster.agentPosition;
			List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int j = 0; j < entities.Count; j++)
			{
				Entity entity = entities[j];
				CreateDamage(DamageData.SourceType.Default, dmgFactor).SetDirection((info.caster.agentPosition - entity.agentPosition).normalized).SetOriginPosition(info.caster.agentPosition).SetElemental(ElementalType.Dark)
					.Dispatch(entity);
				FxPlayNewNetworked(fxHit, entity);
				Knockback.ApplyWithOrigin(info.caster.agentPosition, entity);
			}
			handle.Return();
			yield return new SI.WaitForSeconds(unequipStartTime);
			FxPlayNetworked(fxUnequipStart, info.caster);
			yield return new SI.WaitForSeconds(unequipEndTime);
			FxPlayNetworked(fxUnequipEnd, info.caster);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
