using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_R_Parry_Start : StatusEffect
{
	private float _baseDuration;

	public DewCollider range;

	public GameObject parryEffect;

	public float duration = 2.5f;

	public float selfSlowAmount = 50f;

	public bool cancelable;

	public float uncancellableTime = 0.5f;

	public DewAnimationClip parryAnim;

	public float reducedCooldownPercentage = 0.5f;

	public float parrySuccessLookDuration = 1f;

	public ScalingValue maxEnemiesHit = "4";

	public float hitboxMultiplier = 2f;

	public bool resetMeister = true;

	[NonSerialized]
	public bool allowAnyDirection;

	private Channel _channel;

	private bool _didParry;

	public override bool reuseInRoom => true;

	protected override void Awake()
	{
		base.Awake();
		_baseDuration = duration;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_didParry = false;
		allowAnyDirection = false;
		duration = _baseDuration;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		victim.Control.Rotate(info.rotation, immediately: true, 0.25f);
		DoSlow(selfSlowAmount);
		DoUnstoppable();
		SetTimer(duration);
		ShowOnScreenTimer();
		_channel = new Channel
		{
			duration = duration,
			blockedActions = (Channel.BlockedAction)(7 | (cancelable ? 128 : 0)),
			uncancellableTime = uncancellableTime,
			onCancel = () =>
			{
				if (isActive)
				{
					Destroy();
				}
			},
			onComplete = () =>
			{
				if (isActive)
				{
					Destroy();
				}
			}
		};
		victim.Control.StartChannel(_channel);
		victim.takenDamageProcessor.Add(BlockDamage, 100);
		victim.Control.outerRadius = Mathf.Round(victim.Control.outerRadius * hitboxMultiplier * 100f) / 100f;
		ResetCooldown(victim.Ability.attackAbility);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.takenDamageProcessor.Remove(BlockDamage);
				victim.Control.outerRadius = Mathf.Round(victim.Control.outerRadius / hitboxMultiplier * 100f) / 100f;
			}
			if (_channel.isAlive)
			{
				_channel.Cancel();
			}
		}
	}

	private void BlockDamage(ref DamageData data, Actor actor, Entity target)
	{
		if (data.HasAttr(DamageAttribute.IgnoreDamageImmunity) && !actor.IsDescendantOf<Gem_E_Overload>())
		{
			return;
		}
		if (!allowAnyDirection)
		{
			Vector2 lhs = ((Component)(object)victim).transform.forward.ToXY();
			if ((!data.direction.HasValue || Vector2.Dot(lhs, (-data.direction.Value).ToXY()) <= 0f) && (!data.originPosition.HasValue || Vector2.Dot(lhs, (data.originPosition.Value - victim.position).ToXY()) <= 0f))
			{
				Entity entity = actor.firstEntity;
				if ((UnityEngine.Object)(object)entity == null || Vector2.Dot(lhs, (entity.position - victim.position).ToXY()) <= 0f)
				{
					return;
				}
			}
		}
		data.BlockWithImmunity();
		if (actor is ElementalStatusEffect || _didParry)
		{
			return;
		}
		_didParry = true;
		SetTimer(0.1f);
		HideOnScreenTimer();
		FxPlayNetworked(parryEffect, victim);
		range.transform.SetPositionAndRotation(victim.position, info.rotation);
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		int num = 0;
		int num2 = Mathf.RoundToInt(GetValue(maxEnemiesHit));
		foreach (Entity item in entities)
		{
			CreateAbilityInstance<Ai_R_Parry_Projectile>(victim.position, Quaternion.identity, new CastInfo(victim, item));
			if (resetMeister)
			{
				Se_D_AstridsMasterpieceEnGarde_Exposed se_D_AstridsMasterpieceEnGarde_Exposed = item.Status.FindStatusEffect((Se_D_AstridsMasterpieceEnGarde_Exposed e) => (UnityEngine.Object)(object)e.info.caster == (UnityEngine.Object)(object)victim);
				if ((UnityEngine.Object)(object)se_D_AstridsMasterpieceEnGarde_Exposed != null)
				{
					se_D_AstridsMasterpieceEnGarde_Exposed.chargeCount = se_D_AstridsMasterpieceEnGarde_Exposed.maxCharge;
				}
			}
			num++;
			if (num >= num2)
			{
				break;
			}
		}
		handle.Return();
		if (victim.Status.TryGetStatusEffect<Se_D_AstridsMasterpiecePriorite>(out var effect))
		{
			effect.Reset();
		}
		float amount = data.currentAmount;
		CreateStatusEffect(victim, new CastInfo(info.caster), (Se_R_Parry_End se) =>
		{
			se.blockedDamage = amount;
		});
		AbilityTrigger abilityTrigger = firstTrigger;
		if ((UnityEngine.Object)(object)abilityTrigger != null)
		{
			ApplyCooldownReductionByRatio(abilityTrigger, reducedCooldownPercentage);
		}
		victim.Animation.PlayAbilityAnimation(parryAnim);
		victim.Control.Rotate(info.rotation, immediately: true, parrySuccessLookDuration);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Control.Rotate(info.rotation, immediately: true, 0.25f);
		}
	}

	private void MirrorProcessed()
	{
	}
}
