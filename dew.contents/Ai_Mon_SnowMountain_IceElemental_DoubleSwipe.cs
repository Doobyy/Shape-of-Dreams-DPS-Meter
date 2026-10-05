using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_SnowMountain_IceElemental_DoubleSwipe : AbilityInstance
{
	public int swipeCount;

	public DewAnimationClip[] swipeAnimations;

	public GameObject[] swipeEffects;

	public GameObject[] hitEffects;

	public Dash[] swipeDashes;

	public float[] swipeIntervals;

	public float[] animationOffsetTimes;

	public DewCollider[] swipeRanges;

	public Knockback[] swipeKnockbacks;

	public float coldChance;

	public ScalingValue damage;

	public override bool reuseInRoom => true;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		float num = 0f;
		float[] array = swipeIntervals;
		foreach (float num2 in array)
		{
			num += num2;
		}
		info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = num,
			onCancel = () =>
			{
				if (isActive)
				{
					Destroy();
				}
			}
		});
		StartSequence(AnimationSequence());
		for (int i2 = 0; i2 < swipeCount; i2++)
		{
			FxPlayNetworked(swipeEffects[i2]);
			swipeDashes[i2].ApplyByDirection(info.caster, info.forward, (DispByDestination d) =>
			{
				d.affectedByMovementSpeed = true;
			});
			List<Entity> entities = swipeRanges[i2].GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
			for (int num3 = 0; num3 < entities.Count; num3++)
			{
				Entity entity = entities[num3];
				FxPlayNewNetworked(hitEffects[i2], entity);
				swipeKnockbacks[i2].ApplyWithDirection(info.forward, entity);
				DamageData damageData = DefaultDamage(damage);
				if (Random.value < coldChance)
				{
					damageData.SetElemental(ElementalType.Cold);
				}
				damageData.Dispatch(entity);
			}
			handle.Return();
			yield return new SI.WaitForSeconds(swipeIntervals[i2]);
		}
		Destroy();
	}

	private IEnumerator AnimationSequence()
	{
		for (int i = 0; i < swipeCount; i++)
		{
			info.caster.Animation.PlayAbilityAnimation(swipeAnimations[i]);
			yield return new SI.WaitForSeconds(swipeIntervals[i] + animationOffsetTimes[i]);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		((Component)(object)this).transform.position = info.caster.position;
	}

	private void MirrorProcessed()
	{
	}
}
