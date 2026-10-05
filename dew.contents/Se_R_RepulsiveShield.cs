using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_R_RepulsiveShield : StatusEffect
{
	public DewAnimationClip stoppedClip;

	public DewCollider range;

	public GameObject blockEffect;

	public ScalingValue duration;

	public float selfSlowAmount = 50f;

	public bool cancelable;

	public float rateLimitTime = 0.5f;

	public float rateLimitCount = 2.5f;

	public float hitboxMultiplier = 2f;

	public float uncancellableTime = 0.5f;

	public int targetLimit = 4;

	private float _remainingRate;

	public override bool reuseInRoom => true;

	protected override void OnDisable()
	{
		base.OnDisable();
		_remainingRate = 0f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		victim.Control.LockGamepadRotation();
		victim.Control.Rotate(info.rotation, immediately: true, 0.25f);
		DoSlow(selfSlowAmount);
		DoUnstoppable();
		float value = GetValue(duration);
		SetTimer(value);
		ShowOnScreenTimer();
		Channel channel = new Channel
		{
			duration = value,
			blockedActions = (Channel.BlockedAction)(6 | (cancelable ? 128 : 0)),
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
		victim.Control.StartChannel(channel);
		victim.takenDamageProcessor.Add(BlockDamage, -100);
		victim.Control.outerRadius = Mathf.Round(victim.Control.outerRadius * hitboxMultiplier * 100f) / 100f;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null)
		{
			victim.Control.UnlockGamepadRotation();
			victim.takenDamageProcessor.Remove(BlockDamage);
			victim.Control.outerRadius = Mathf.Round(victim.Control.outerRadius / hitboxMultiplier * 100f) / 100f;
			victim.Animation.StopAbilityAnimation(stoppedClip);
		}
	}

	private void BlockDamage(ref DamageData data, Actor actor, Entity target)
	{
		Entity entity = actor.firstEntity;
		if ((Object)(object)entity == null || !victim.CheckEnemyOrNeutral(entity))
		{
			return;
		}
		Vector2 lhs = ((Component)(object)victim).transform.forward.ToXY();
		if (Vector2.Dot(lhs, (entity.position - victim.position).ToXY()) <= 0f && (!data.direction.HasValue || Vector2.Dot(lhs, (-data.direction.Value).ToXY()) <= 0f) && (!data.originPosition.HasValue || Vector2.Dot(lhs, (data.originPosition.Value - victim.position).ToXY()) <= 0f))
		{
			return;
		}
		data.BlockWithImmunity();
		FxPlayNewNetworked(blockEffect, victim);
		if (_remainingRate < 1f)
		{
			return;
		}
		_remainingRate--;
		if ((Object)(object)actor.FindFirstAncestorOfType<Se_R_RepulsiveShield>() != null)
		{
			return;
		}
		range.transform.SetPositionAndRotation(victim.position, victim.rotation);
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		int num = 0;
		foreach (Entity item in entities)
		{
			if (num < targetLimit)
			{
				num++;
				CreateAbilityInstance<Ai_R_RepulsiveShield_Projectile>(victim.position, Quaternion.identity, new CastInfo(victim, item));
			}
		}
		handle.Return();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Control.Rotate(info.rotation, immediately: true, 0.25f);
			_remainingRate = Mathf.MoveTowards(_remainingRate, rateLimitCount, dt * rateLimitCount / rateLimitTime);
		}
	}

	private void MirrorProcessed()
	{
	}
}
