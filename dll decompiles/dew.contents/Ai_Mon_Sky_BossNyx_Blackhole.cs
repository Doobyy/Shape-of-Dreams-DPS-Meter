using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Sky_BossNyx_Blackhole : AbilityInstance
{
	public GameObject displaceEffect;

	public GameObject startTelegraphEffect;

	public DewAnimationClip animDisplace;

	public float displaceDuration;

	public DewEase displaceEase;

	public GameObject blackholeStartEffect;

	public GameObject blackholeRepeatedEffect;

	public float repeatedEffectInterval;

	public GameObject blackholeEndEffect;

	public float blackholeDuration;

	public float tickDamageRadius = 3.5f;

	public int blackholeStarfallCount;

	public float tickInterval;

	public float tickDamageRatio;

	public AnimationCurve dmgMultiplierOverLifetime;

	public Vector2 distanceBounds;

	public AnimationCurve attractStrengthByDist;

	public AnimationCurve attractStrengthMulOverLifetime;

	public float explodeDamageRadius = 4f;

	public float explodeDamageRatio;

	public DewAnimationClip animEnd;

	public float monsterDmgMultiplier;

	public float afterBlackholeDelay;

	[SyncVar]
	private bool _isBlackholeOn;

	[SyncVar]
	private float _blackholeStartNetworkTime;

	private float _lastTickTime;

	private bool _didTurnOffRenderer;

	private float _lastRepeatedEffectTime;

	public bool Network_isBlackholeOn
	{
		get
		{
			return _isBlackholeOn;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isBlackholeOn, 64uL, (Action<bool, bool>)null);
		}
	}

	public float Network_blackholeStartNetworkTime
	{
		get
		{
			return _blackholeStartNetworkTime;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _blackholeStartNetworkTime, 128uL, (Action<float, float>)null);
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		CreateBasicEffect(info.caster, new UnstoppableEffect(), 1000f).DestroyOnDestroy(this);
		DestroyOnDeath(info.caster);
		if (SingletonBehaviour<Sky_BossRoomCenter>.instance != null)
		{
			info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = true,
				destination = SingletonBehaviour<Sky_BossRoomCenter>.instance.transform.position,
				duration = displaceDuration,
				ease = displaceEase,
				isCanceledByCC = false,
				isFriendly = true,
				rotateForward = false
			});
		}
		info.caster.Animation.PlayAbilityAnimation(animDisplace);
		info.caster.Control.Rotate(Vector3.back, immediately: false, 2f);
		FxPlayNetworked(displaceEffect, info.caster);
		FxPlayNewNetworked(startTelegraphEffect, (SingletonBehaviour<Sky_BossRoomCenter>.instance != null) ? SingletonBehaviour<Sky_BossRoomCenter>.instance.transform.position : info.caster.position, null);
		float num = displaceDuration + blackholeDuration + afterBlackholeDelay;
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
		info.caster.Control.Rotate(Vector3.back, immediately: false, num);
		yield return new SI.WaitForSeconds(displaceDuration);
		FxStopNetworked(displaceEffect);
		FxPlayNetworked(blackholeStartEffect, info.caster);
		Network_isBlackholeOn = true;
		Network_blackholeStartNetworkTime = (float)NetworkTime.time;
		_didTurnOffRenderer = true;
		info.caster.Visual.DisableRenderers();
		for (int i = 0; i < blackholeStarfallCount; i++)
		{
			CreateAbilityInstance(info.caster.position, null, info, (Ai_Mon_Sky_BossNyx_Starfall s) =>
			{
				s.starfallWavaCount = 1;
				s.starfallCount = 30;
			});
			yield return new SI.WaitForSeconds(blackholeDuration / (float)blackholeStarfallCount);
		}
		Network_isBlackholeOn = false;
		FxStopNetworked(blackholeStartEffect);
		FxPlayNetworked(blackholeEndEffect, info.caster);
		if (_didTurnOffRenderer)
		{
			info.caster.Visual.EnableRenderers();
			_didTurnOffRenderer = false;
		}
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.caster.position, explodeDamageRadius);
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			Entity entity = list[num2];
			if (!((UnityEngine.Object)(object)entity == (UnityEngine.Object)(object)info.caster) && !entity.IsNullInactiveDeadOrKnockedOut())
			{
				float num3 = explodeDamageRatio * entity.maxHealth;
				if (entity is Monster)
				{
					num3 *= monsterDmgMultiplier;
				}
				DefaultDamage(num3).SetOriginPosition(position).Dispatch(entity);
			}
		}
		info.caster.Animation.PlayAbilityAnimation(animEnd);
		handle.Return();
		yield return new SI.WaitForSeconds(afterBlackholeDelay);
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(displaceEffect);
			FxStopNetworked(blackholeStartEffect);
			if (_didTurnOffRenderer && (UnityEngine.Object)(object)info.caster != null)
			{
				info.caster.Visual.EnableRenderers();
				_didTurnOffRenderer = false;
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!_isBlackholeOn)
		{
			return;
		}
		float time = Mathf.Clamp01((float)(NetworkTime.time - (double)_blackholeStartNetworkTime) / blackholeDuration);
		if (Time.time - _lastRepeatedEffectTime > repeatedEffectInterval)
		{
			_lastRepeatedEffectTime = Time.time;
			FxPlay(blackholeRepeatedEffect);
		}
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (!((UnityEngine.Object)(object)allEntity == (UnityEngine.Object)(object)info.caster) && !allEntity.IsNullInactiveDeadOrKnockedOut() && allEntity.Control.isLocalMovementProcessor && !allEntity.Control.isDisplacing && !allEntity.Status.hasCrowdControlImmunity)
			{
				float time2 = Mathf.Clamp01((Vector2.Distance(position.ToXY(), allEntity.agentPosition.ToXY()) - distanceBounds.x) / (distanceBounds.y - distanceBounds.x));
				float num = attractStrengthByDist.Evaluate(time2);
				num *= attractStrengthMulOverLifetime.Evaluate(time);
				allEntity.Control.SetAgentPosition(allEntity.agentPosition + (position - allEntity.agentPosition).normalized * (num * dt));
			}
		}
		if (!((NetworkBehaviour)this).isServer || !(Time.time - _lastTickTime > tickInterval))
		{
			return;
		}
		_lastTickTime = Time.time;
		ListReturnHandle<Entity> handle;
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, info.caster.position, tickDamageRadius))
		{
			if (!((UnityEngine.Object)(object)item == (UnityEngine.Object)(object)info.caster) && !item.IsNullInactiveDeadOrKnockedOut())
			{
				float num2 = tickDamageRatio * item.maxHealth * dmgMultiplierOverLifetime.Evaluate(time);
				if (item is Monster)
				{
					num2 *= monsterDmgMultiplier;
				}
				DefaultDamage(num2).SetOriginPosition(position).Dispatch(item);
			}
		}
		handle.Return();
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		position = info.caster.position;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, _isBlackholeOn);
			NetworkWriterExtensions.WriteFloat(writer, _blackholeStartNetworkTime);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isBlackholeOn);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _blackholeStartNetworkTime);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isBlackholeOn, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _blackholeStartNetworkTime, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isBlackholeOn, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _blackholeStartNetworkTime, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
