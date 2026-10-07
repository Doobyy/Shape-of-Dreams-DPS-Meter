using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public abstract class DamageInstance : AbilityInstance
{
	private class Ad_DuplicateCheck
	{
		public Dictionary<Type, float> hitTimes = new Dictionary<Type, float>();
	}

	public enum OriginType
	{
		None,
		Instance,
		Caster
	}

	public enum DuplicateCheckType
	{
		HitOnce = 0,
		CooldownPerInstance = 1,
		CooldownSharedByInstanceType = 2,
		DontCheck = -1
	}

	public GameObject hitEffect;

	public GameObject mainEffect;

	public DewCollider range;

	public AbilityTargetValidator hittable;

	public DuplicateCheckType duplicateCheck;

	public float cooldownTime = 0.25f;

	public bool cancelIfCasterDead = true;

	public bool destroyWhenDone = true;

	public DamageData.SourceType type;

	public ScalingValue dmgFactor;

	public bool multiplyDamageByMaxHp;

	public float procCoefficient = 1f;

	public OriginType origin;

	public bool isDamageOverTime;

	public bool forceMergeNumbers;

	public float attackEffect;

	public AttackEffectType attackEffectType = AttackEffectType.Others;

	public bool useChannel;

	public bool destroyIfChannelCanceled;

	public ChannelData channelData;

	public float knockupAmount;

	public bool applyElemental;

	public float elementalChance = 1f;

	public ElementalType elemental;

	public bool doKnockback;

	public bool useDirectionOfCast;

	public float closeEnemyMultiplier = 1f;

	public float closeEnemyDistanceThreshold = 4f;

	public Knockback knockbackSettings;

	public float heroDamageMultiplier = 1f;

	public float monsterDamageMultiplier = 1f;

	public bool affectedByAttackSpeed;

	[SyncVar]
	public float strengthMultiplier = 1f;

	public SafeAction<Entity> onHit;

	public SafeAction onChannelCompleted;

	public SafeAction onChannelCanceled;

	private Dictionary<Entity, float> _hitEntities;

	private Dictionary<Entity, float> _hitEntitiesStore;

	private bool _isCooldownCheck
	{
		get
		{
			DuplicateCheckType duplicateCheckType = duplicateCheck;
			return duplicateCheckType == DuplicateCheckType.CooldownSharedByInstanceType || duplicateCheckType == DuplicateCheckType.CooldownPerInstance;
		}
	}

	protected virtual float DamageTime => Time.time;

	protected virtual float DuplicateCooldown => cooldownTime;

	public float NetworkstrengthMultiplier
	{
		get
		{
			return strengthMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref strengthMultiplier, 64uL, (Action<float, float>)null);
		}
	}

	public override void ClearPooledEventsAndProcessors()
	{
		base.ClearPooledEventsAndProcessors();
		onHit?.Clear();
		onChannelCompleted?.Clear();
		onChannelCanceled?.Clear();
	}

	protected virtual float GetDuplicateStamp(bool hadPrevious, float previousStamp)
	{
		return DamageTime;
	}

	protected override void OnCreate()
	{
		float num = ((affectedByAttackSpeed && !info.caster.IsNullInactiveDeadOrKnockedOut()) ? info.caster.Status.attackSpeedMultiplier : 1f);
		if (affectedByAttackSpeed)
		{
			if ((bool)startEffectNoStop)
			{
				DewEffect.ApplySpeedMultiplier(startEffectNoStop, num);
			}
			if ((bool)startEffect)
			{
				DewEffect.ApplySpeedMultiplier(startEffect, num);
			}
		}
		float duration = channelData.duration / num;
		if (((NetworkBehaviour)this).isServer)
		{
			if (_hitEntitiesStore == null)
			{
				_hitEntitiesStore = new Dictionary<Entity, float>();
			}
			_hitEntitiesStore.Clear();
			_hitEntities = _hitEntitiesStore;
		}
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (mainEffect != null)
		{
			FxPlayNetworked(mainEffect);
		}
		if (!useChannel || info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			return;
		}
		Channel channel = channelData.Get().AddOnCancel(() =>
		{
			try
			{
				OnChannelCanceled();
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			onChannelCanceled?.Invoke();
			if (isActive && destroyIfChannelCanceled)
			{
				Destroy();
			}
		}).AddOnComplete(() =>
		{
			try
			{
				OnChannelCompleted();
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			onChannelCompleted?.Invoke();
		});
		channel.duration = duration;
		try
		{
			OnBeforeDispatchChannel(channel);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		channel.Dispatch(info.caster);
	}

	protected virtual void OnBeforeDispatchChannel(Channel channel)
	{
	}

	protected virtual void OnChannelCanceled()
	{
	}

	protected virtual void OnChannelCompleted()
	{
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && _hitEntities != null)
		{
			_hitEntities.Clear();
			_hitEntities = null;
		}
	}

	protected bool CheckShouldBeDestroyed()
	{
		if (cancelIfCasterDead && info.caster != null && info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			return true;
		}
		return false;
	}

	[Server]
	protected void DoCollisionChecks()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DamageInstance::DoCollisionChecks()' called when server was not active");
		}
		else
		{
			OnCollisionCheck();
		}
	}

	protected virtual void OnCollisionCheck()
	{
		if (!isActive || _hitEntities == null)
		{
			return;
		}
		if (!ShouldUseDefaultAbilityTargetValidator())
		{
			List<Entity> entities = range.GetEntities(out var handle);
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
			return;
		}
		List<Entity> entities2 = range.GetEntities(out var handle2, hittable, info.caster);
		for (int j = 0; j < entities2.Count; j++)
		{
			Entity entity2 = entities2[j];
			if (!IsDuplicate(entity2) && OnValidateTarget(entity2))
			{
				AddToDuplicateTracker(entity2);
				OnHit(entity2);
			}
		}
		handle2.Return();
	}

	protected virtual bool ShouldUseDefaultAbilityTargetValidator()
	{
		return info.caster != null;
	}

	protected virtual bool OnValidateTarget(Entity entity)
	{
		return true;
	}

	protected virtual void OnHit(Entity entity)
	{
		onHit?.Invoke(entity);
		if (hitEffect != null)
		{
			FxPlayNewNetworked(hitEffect, entity);
		}
		float num = GetValue(dmgFactor);
		if (multiplyDamageByMaxHp)
		{
			num *= entity.maxHealth;
		}
		if (entity is Hero)
		{
			num *= heroDamageMultiplier;
		}
		if (entity is Monster)
		{
			num *= monsterDamageMultiplier;
		}
		DamageData dmg = CreateDamage(type, num, Mathf.Clamp01(procCoefficient * strengthMultiplier));
		if (applyElemental && UnityEngine.Random.value < elementalChance)
		{
			dmg.SetElemental(elemental);
		}
		switch (origin)
		{
		case OriginType.Instance:
			dmg.SetOriginPosition(position);
			break;
		case OriginType.Caster:
			dmg.SetOriginPosition(info.caster.position);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case OriginType.None:
			break;
		}
		if (doKnockback)
		{
			float distance = knockbackSettings.distance;
			if (dmg.originPosition.HasValue && Vector2.Distance(dmg.originPosition.Value, entity.position) < closeEnemyDistanceThreshold)
			{
				knockbackSettings.distance *= closeEnemyMultiplier;
			}
			if (useDirectionOfCast)
			{
				knockbackSettings.ApplyWithDirection(info.forward, entity);
			}
			else if (dmg.originPosition.HasValue)
			{
				knockbackSettings.ApplyWithOrigin(dmg.originPosition.Value, entity);
			}
			knockbackSettings.distance = distance;
		}
		if (knockupAmount > 0.001f && !entity.Status.hasCrowdControlImmunity)
		{
			entity.Visual.KnockUp(knockupAmount * strengthMultiplier, isFriendly: false);
		}
		dmg.ApplyRawMultiplier(strengthMultiplier);
		if (isDamageOverTime)
		{
			dmg.SetAttr(DamageAttribute.DamageOverTime);
		}
		if (forceMergeNumbers)
		{
			dmg.SetAttr(DamageAttribute.ForceMergeNumber);
		}
		if (attackEffect > 0.001f)
		{
			dmg.DoAttackEffect(attackEffectType, attackEffect);
		}
		OnBeforeDispatchDamage(ref dmg, entity);
		dmg.Dispatch(entity, chain);
	}

	protected virtual void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
	}

	protected bool IsDuplicate(Entity ent)
	{
		switch (duplicateCheck)
		{
		case DuplicateCheckType.HitOnce:
			return _hitEntities.ContainsKey(ent);
		case DuplicateCheckType.CooldownPerInstance:
		{
			if (!_hitEntities.TryGetValue(ent, out var value2))
			{
				return false;
			}
			return DamageTime - value2 < DuplicateCooldown;
		}
		case DuplicateCheckType.CooldownSharedByInstanceType:
		{
			if (!ent.TryGetData<Ad_DuplicateCheck>(out var data))
			{
				return false;
			}
			if (!data.hitTimes.TryGetValue(((object)this).GetType(), out var value))
			{
				return false;
			}
			return Time.time - value < cooldownTime;
		}
		case DuplicateCheckType.DontCheck:
			return false;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	protected void AddToDuplicateTracker(Entity ent)
	{
		switch (duplicateCheck)
		{
		case DuplicateCheckType.HitOnce:
		case DuplicateCheckType.CooldownPerInstance:
		{
			_hitEntities[ent] = GetDuplicateStamp(_hitEntities.TryGetValue(ent, out var value), value);
			break;
		}
		case DuplicateCheckType.CooldownSharedByInstanceType:
		{
			if (!ent.TryGetData<Ad_DuplicateCheck>(out var data))
			{
				data = new Ad_DuplicateCheck();
				ent.AddData(data);
			}
			data.hitTimes[((object)this).GetType()] = Time.time;
			break;
		}
		case DuplicateCheckType.DontCheck:
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, strengthMultiplier);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, strengthMultiplier);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref strengthMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref strengthMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
