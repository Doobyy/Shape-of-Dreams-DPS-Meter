using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_U_Hysteria_Claw : InstantDamageInstance
{
	public ScalingValue healAmount;

	public float healAmpThreshold;

	public float healAmp;

	public GameObject fxLeft;

	public GameObject fxRight;

	public DewAnimationClip animLeft;

	public DewAnimationClip animRight;

	public Dash dash;

	[NonSerialized]
	public bool isRight;

	private bool _didHit;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_didHit = false;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			dash.ApplyByDirection(info.caster, info.forward);
			if (!isRight)
			{
				FxPlayNetworked(fxLeft);
				info.caster.Animation.PlayAbilityAnimation(animLeft);
			}
			else
			{
				FxPlayNetworked(fxRight);
				info.caster.Animation.PlayAbilityAnimation(animRight);
			}
		}
	}

	protected override void OnCollisionCheck()
	{
		base.OnCollisionCheck();
		List<Entity> entities = range.GetEntities(out var handle, hittable, info.caster, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		for (int i = 0; i < entities.Count; i++)
		{
			Entity entity = entities[i];
			if (!IsDuplicate(entity) && OnValidateTarget(entity))
			{
				AddToDuplicateTracker(entity);
				OnHit(entity);
			}
		}
		handle.Return();
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (!_didHit)
		{
			_didHit = true;
			dmg.DoAttackEffect(AttackEffectType.Others);
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		HealData healData = Heal(healAmount).SetCanMerge();
		if (info.caster.normalizedHealth < healAmpThreshold)
		{
			healData.SetCrit();
			healData.ApplyAmplification(healAmp);
		}
		healData.Dispatch(info.caster);
	}

	private void MirrorProcessed()
	{
	}
}
