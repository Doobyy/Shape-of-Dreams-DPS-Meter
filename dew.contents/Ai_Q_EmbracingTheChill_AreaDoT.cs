using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Q_EmbracingTheChill_AreaDoT : TickDamageInstance
{
	[Header("Skill Settings")]
	public ScalingValue shieldPerTick;

	public float shieldDuration = 3f;

	public AnimationCurve sizeCurveByTime;

	[NonSerialized]
	[SyncVar]
	public float sizeMultiplier = 1f;

	[NonSerialized]
	public bool isChasingTarget;

	[NonSerialized]
	public float chasingTargetFindRadius = 15f;

	[NonSerialized]
	public float chasingSpeed = 5f;

	private Entity _chasingEntity;

	[NonSerialized]
	public bool doSelfDamage;

	[NonSerialized]
	public float takeDamageRatio;

	[NonSerialized]
	public float damageAmp;

	[NonSerialized]
	public bool hasInfiniteDuration;

	[NonSerialized]
	public float baseScale = 1f;

	private float _originDuration;

	private int _originTicks;

	public float NetworksizeMultiplier
	{
		get
		{
			return sizeMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref sizeMultiplier, 128uL, (Action<float, float>)null);
		}
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		position = info.point;
		_originDuration = duration;
		_originTicks = ticks;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (isChasingTarget)
			{
				FindChasableTarget();
			}
			if (doSelfDamage)
			{
				InitSelfDamageSettings();
			}
			if (hasInfiniteDuration)
			{
				InitInfiniteDurationSettings();
			}
		}
	}

	protected override void OnHit(Entity entity)
	{
		if (info.caster.CheckEnemyOrNeutral(entity))
		{
			base.OnHit(entity);
			return;
		}
		if ((bool)hitEffect)
		{
			FxPlayNewNetworked(hitEffect, entity);
		}
		if (doSelfDamage && (UnityEngine.Object)(object)entity == (UnityEngine.Object)(object)info.caster)
		{
			ApplySelfDamage();
		}
		else
		{
			GiveOrUpdateShield(entity);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			float time = (hasInfiniteDuration ? ((Time.time - creationTime) / _originDuration) : normalizedDuration);
			NetworksizeMultiplier = baseScale * sizeCurveByTime.Evaluate(time);
			if (hasInfiniteDuration)
			{
				CheckInCombat();
			}
			if (isChasingTarget)
			{
				ChaseTarget(dt);
			}
		}
		((Component)(object)this).transform.localScale = Vector3.one * sizeMultiplier;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !hasInfiniteDuration)
		{
			CreateAbilityInstance(position, null, new CastInfo(info.caster), (Ai_Q_EmbracingTheChill_Explosion ai) =>
			{
				ai.NetworksizeMultiplier = sizeMultiplier;
				ai.isSelfDamage = doSelfDamage;
				ai.dmgFactor *= 1f + damageAmp;
				ai.takeDamageRatio = takeDamageRatio;
			});
		}
	}

	private void GiveOrUpdateShield(Entity target)
	{
		Se_GenericShield_OneShot se_GenericShield_OneShot = target.Status.FindStatusEffect((Se_GenericShield_OneShot se) => (UnityEngine.Object)(object)se.parentActor == (UnityEngine.Object)(object)this);
		if ((bool)(UnityEngine.Object)(object)se_GenericShield_OneShot)
		{
			se_GenericShield_OneShot.AddAmount(GetValue(shieldPerTick));
			se_GenericShield_OneShot.ResetTimer();
		}
		else
		{
			GiveShield(target, GetValue(shieldPerTick), shieldDuration);
		}
	}

	private void FindChasableTarget()
	{
		List<Entity> list = DewPhysics.OverlapCircleAllEntities(out var handle, position, chasingTargetFindRadius, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.DistanceFromCenter
		});
		_chasingEntity = ((list.Count > 0) ? list.First() : info.caster);
		handle.Return();
	}

	private void ChaseTarget(float dt)
	{
		if ((UnityEngine.Object)(object)_chasingEntity == null)
		{
			FindChasableTarget();
		}
		if (!_chasingEntity.IsNullInactiveDeadOrKnockedOut())
		{
			Vector2 vector = _chasingEntity.position.ToXY() - position.ToXY();
			float magnitude = vector.magnitude;
			if (magnitude <= chasingSpeed * dt)
			{
				position = _chasingEntity.position;
				return;
			}
			Vector2 v = vector / magnitude;
			position += v.ToXZ() * (chasingSpeed * dt);
		}
		else
		{
			_chasingEntity = null;
		}
	}

	private void InitSelfDamageSettings()
	{
		dmgFactor *= 1f + damageAmp;
	}

	private void ApplySelfDamage()
	{
		Damage(dmgFactor, Mathf.Clamp01(procCoefficient * strengthMultiplier)).SetElemental(ElementalType.Cold).ApplyRawMultiplier(takeDamageRatio).Dispatch(info.caster);
	}

	private void InitInfiniteDurationSettings()
	{
		ticks = int.MaxValue;
	}

	private void CheckInCombat()
	{
		Hero hero = info.caster as Hero;
		if (!((UnityEngine.Object)(object)hero != null) || !hero.isInCombat)
		{
			hasInfiniteDuration = false;
			ticks = _originTicks;
			if (ticks <= doneTicks)
			{
				DestroyIfActive();
			}
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
			NetworkWriterExtensions.WriteFloat(writer, sizeMultiplier);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, sizeMultiplier);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sizeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sizeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
