using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_SliceThroat_Atk : AbilityInstance
{
	public float postDelay;

	public float dmgDelay;

	public float healAmountPerHit;

	public float outerDmgAmp;

	public ScalingValue dmgFactor;

	public DewAnimationClip atkAnim;

	public DewCollider range;

	public DewCollider outerRange;

	public Knockback knockback;

	public GameObject fxHit;

	public GameObject fxOuterHit;

	public float secondAtkChance;

	public float secondAtkDelay;

	public GameObject fxSecondAtkPrepare;

	public GameObject fxSecondAtkTelegraph;

	public DewAnimationClip secondAtkPrepareAnim;

	public DewAnimationClip secondAtkAnim;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		info.caster.Animation.PlayAbilityAnimation(atkAnim);
		yield return new SI.WaitForSeconds(dmgDelay);
		int num = 0;
		List<Entity> entities = outerRange.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		float num2 = info.caster.Status.missingHealth * healAmountPerHit;
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			if (!entity.Status.hasDamageImmunity)
			{
				num++;
			}
			CreateDamage(DamageData.SourceType.Default, GetValue(dmgFactor) * outerDmgAmp).SetOriginPosition(info.caster.position).SetAttr(DamageAttribute.IsCrit).SetElemental(ElementalType.Dark)
				.SetDirection(info.forward)
				.Dispatch(entity);
			knockback.ApplyWithDirection(info.forward, entity);
			FxPlayNewNetworked(fxOuterHit, entity);
		}
		Heal((float)num * num2).Dispatch(info.caster);
		if (range != null)
		{
			List<Entity> entities2 = range.GetEntities(out var handle2, tvDefaultHarmfulEffectTargets);
			for (int j = 0; j < entities2.Count; j++)
			{
				if (!entities.Contains(entities2[j]))
				{
					Entity entity2 = entities2[j];
					CreateDamage(DamageData.SourceType.Default, dmgFactor).SetDirection(info.forward).SetOriginPosition(info.caster.position).Dispatch(entity2);
					knockback.ApplyWithDirection(info.forward, entity2);
					FxPlayNewNetworked(fxHit, entity2);
				}
			}
			handle2.Return();
		}
		handle.Return();
		info.caster.Control.StartDaze(secondAtkDelay + postDelay + 0.5f);
		yield return new SI.WaitForSeconds(0.5f);
		if (Random.value < secondAtkChance * (0.5f + NetworkedManagerBase<GameManager>.instance.difficulty.specialSkillChanceMultiplier * 0.5f))
		{
			ChannelAttack();
		}
		yield return new SI.WaitForSeconds(postDelay + secondAtkDelay);
		Destroy();
	}

	private void ChannelAttack()
	{
		Vector3 pos = info.caster.position;
		Quaternion rot = Quaternion.Euler(0f, AbilityTrigger.PredictAngle_Simple(info.caster, NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), Dew.GetClosestAliveHero(pos, fallbackToDead: true, info.caster), pos, secondAtkDelay), 0f);
		info.caster.Control.Rotate(rot, immediately: true);
		FxPlayNetworked(fxSecondAtkPrepare, info.caster);
		FxPlayNetworked(fxSecondAtkTelegraph, pos, rot);
		info.caster.Animation.PlayAbilityAnimation(secondAtkPrepareAnim);
		info.caster.Control.StartChannel(new Channel
		{
			duration = secondAtkDelay,
			blockedActions = Channel.BlockedAction.Everything,
			onComplete = () =>
			{
				info.caster.Animation.PlayAbilityAnimation(secondAtkAnim);
				CreateAbilityInstance<Ai_Mon_Special_BossMaw_SliceThroat_SecondAtk>(pos, rot, new CastInfo(info.caster));
				info.caster.Control.StartDaze(postDelay);
			},
			uncancellableTime = secondAtkDelay
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxSecondAtkPrepare);
			FxStopNetworked(fxSecondAtkTelegraph);
		}
	}

	private void MirrorProcessed()
	{
	}
}
