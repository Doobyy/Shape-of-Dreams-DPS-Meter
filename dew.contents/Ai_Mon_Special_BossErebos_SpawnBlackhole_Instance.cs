using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossErebos_SpawnBlackhole_Instance : AbilityInstance
{
	public float blackholeDuration;

	public GameObject fxBlackholeStart;

	public GameObject fxBlackholeEnd;

	public GameObject fxWhiteholeStart;

	public GameObject fxWhiteholeEnd;

	public float tickInterval;

	public float tickDamageRadius;

	public float tickDamageRatio;

	public AnimationCurve dmgMultiplierOverLifetime;

	public Vector2 distanceBounds;

	public AnimationCurve attractStrengthByDist;

	public AnimationCurve attractStrengthMulOverLifetime;

	public float explodeDamageRadius = 4f;

	public float explodeDamageRatio;

	internal bool isWhitehole;

	[SyncVar]
	private bool _isBlackholeOn;

	[SyncVar]
	private float _blackholeStartNetworkTime;

	private float _lastTickTime;

	private float _lastRepeatedEffectTime;

	private GameObject _fxStart;

	private GameObject _fxEnd;

	private AnimationCurve _baseAttractStrengthByDist;

	private AnimationCurve _baseAttractStrengthMulOverLifetime;

	private float _baseBlackholeDuration;

	private bool _cachedBase;

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

	protected override void Awake()
	{
		base.Awake();
		if (!_cachedBase)
		{
			_baseAttractStrengthByDist = attractStrengthByDist;
			_baseAttractStrengthMulOverLifetime = attractStrengthMulOverLifetime;
			_baseBlackholeDuration = blackholeDuration;
			_cachedBase = true;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		isWhitehole = false;
		if (_cachedBase)
		{
			attractStrengthByDist = _baseAttractStrengthByDist;
			attractStrengthMulOverLifetime = _baseAttractStrengthMulOverLifetime;
			blackholeDuration = _baseBlackholeDuration;
		}
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_lastTickTime = 0f;
		if (isWhitehole)
		{
			_fxStart = fxWhiteholeStart;
			_fxEnd = fxWhiteholeEnd;
		}
		else
		{
			_fxStart = fxBlackholeStart;
			_fxEnd = fxBlackholeEnd;
		}
		Network_blackholeStartNetworkTime = (float)NetworkTime.time;
		Network_isBlackholeOn = true;
		FxPlayNetworked(_fxStart, info.point, Quaternion.identity);
		yield return new SI.WaitForSeconds(blackholeDuration);
		Network_isBlackholeOn = false;
		FxStopNetworked(_fxStart);
		FxPlayNetworked(_fxEnd, info.point, Quaternion.identity);
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, info.point, explodeDamageRadius);
		for (int i = 0; i < list.Count; i++)
		{
			Entity entity = list[i];
			if (!((UnityEngine.Object)(object)entity == (UnityEngine.Object)(object)info.caster) && !(entity is Monster) && !entity.IsNullInactiveDeadOrKnockedOut())
			{
				float amount = explodeDamageRatio * entity.maxHealth;
				DefaultDamage(amount).SetOriginPosition(position).Dispatch(entity);
			}
		}
		handle.Return();
		Destroy();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(_fxStart);
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
		foreach (Entity allEntity in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (!((UnityEngine.Object)(object)allEntity == (UnityEngine.Object)(object)info.caster) && !(allEntity is Monster) && !allEntity.IsNullInactiveDeadOrKnockedOut() && allEntity.Control.isLocalMovementProcessor && !allEntity.Control.isDisplacing && !allEntity.Status.hasCrowdControlImmunity)
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
		foreach (Entity item in DewPhysics.OverlapCircleAllEntities(out handle, info.point, tickDamageRadius))
		{
			if (!((UnityEngine.Object)(object)item == (UnityEngine.Object)(object)info.caster) && !(item is Monster) && !item.IsNullInactiveDeadOrKnockedOut())
			{
				float amount = tickDamageRatio * item.maxHealth * dmgMultiplierOverLifetime.Evaluate(time);
				DefaultDamage(amount).SetOriginPosition(position).Dispatch(item);
			}
		}
		handle.Return();
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
