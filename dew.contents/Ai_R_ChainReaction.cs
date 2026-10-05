using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_R_ChainReaction : TickDamageInstance
{
	public ScalingValue healPerEnemy;

	public float bossAmp = 1.5f;

	[NonSerialized]
	public bool disableHeal;

	[NonSerialized]
	public float globalTravelerHealMultiplier;

	[NonSerialized]
	[SyncVar]
	public float chargedRangeMultiplier = 1f;

	[NonSerialized]
	public float chargedEffectMultiplier = 1f;

	private OnScreenTimerHandle _handle;

	private Vector3 _baseRangeScale;

	private Vector3 _baseFxLoopScale;

	private Vector3 _baseEndEffectScale;

	public override bool reuseInRoom => true;

	public float NetworkchargedRangeMultiplier
	{
		get
		{
			return chargedRangeMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref chargedRangeMultiplier, 128uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseRangeScale = range.transform.localScale;
		_baseFxLoopScale = fxLoop.transform.localScale;
		_baseEndEffectScale = endEffect.transform.localScale;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		disableHeal = false;
		globalTravelerHealMultiplier = 0f;
		NetworkchargedRangeMultiplier = 1f;
		chargedEffectMultiplier = 1f;
		range.transform.localScale = _baseRangeScale;
		fxLoop.transform.localScale = _baseFxLoopScale;
		endEffect.transform.localScale = _baseEndEffectScale;
	}

	protected override void OnCreate()
	{
		position = info.point;
		base.OnCreate();
		if (!Mathf.Approximately(chargedRangeMultiplier, 1f))
		{
			range.transform.localScale = _baseRangeScale * chargedRangeMultiplier;
			fxLoop.transform.localScale = _baseFxLoopScale * chargedRangeMultiplier;
			endEffect.transform.localScale = _baseEndEffectScale * chargedRangeMultiplier;
		}
		if (((NetworkBehaviour)info.caster).isOwned)
		{
			_handle = ShowOnScreenTimerLocally(new OnScreenTimerHandle
			{
				fillAmountGetter = () => (Time.time - creationTime) / duration
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_handle != null)
		{
			HideOnScreenTimerLocally(_handle);
			_handle = null;
		}
	}

	protected override void OnCollisionCheck()
	{
		base.OnCollisionCheck();
		if (disableHeal)
		{
			return;
		}
		List<Entity> entities = range.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		float num = 0f;
		foreach (Entity item in entities)
		{
			if (!item.IsNullInactiveDeadOrKnockedOut() && !item.Status.hasDamageImmunity)
			{
				num = ((!item.IsAnyBoss()) ? (num + 1f) : (num + (1f + bossAmp)));
			}
		}
		if (num <= 0f)
		{
			handle.Return();
			return;
		}
		float amount = GetValue(healPerEnemy) * num * chargedEffectMultiplier;
		List<Entity> entities2 = range.GetEntities(out var handle2, tvDefaultUsefulEffectTargets, new CollisionCheckSettings
		{
			includeUncollidable = true
		});
		foreach (Entity item2 in entities2)
		{
			if (!item2.IsNullInactiveDeadOrKnockedOut())
			{
				Heal(amount).SetCanMerge().Dispatch(item2);
				FxPlayNewNetworked(hitEffect, item2);
			}
		}
		if (globalTravelerHealMultiplier > 0f)
		{
			foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
			{
				if (!gamePlayer.hero.IsNullInactiveDeadOrKnockedOut() && !entities2.Contains(gamePlayer.hero))
				{
					Heal(amount).ApplyRawMultiplier(globalTravelerHealMultiplier).SetCanMerge().Dispatch(gamePlayer.hero);
					FxPlayNewNetworked(hitEffect, gamePlayer.hero);
				}
			}
		}
		handle2.Return();
		handle.Return();
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (chargedEffectMultiplier != 1f)
		{
			dmg.ApplyRawMultiplier(chargedEffectMultiplier);
		}
		if (target.IsAnyBoss())
		{
			dmg.ApplyAmplification(bossAmp).SetAttr(DamageAttribute.IsCrit);
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
			NetworkWriterExtensions.WriteFloat(writer, chargedRangeMultiplier);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, chargedRangeMultiplier);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref chargedRangeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref chargedRangeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
