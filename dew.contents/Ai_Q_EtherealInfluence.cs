using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Q_EtherealInfluence : AbilityInstance
{
	public DewCollider explodeRange;

	public GameObject explodeEffect;

	public GameObject explodeHitEffect;

	public GameObject fxDashMode;

	public ScalingValue explodeDamage;

	public float explodeProcCoefficient;

	public float explodeAmpOnSingleTarget = 0.75f;

	[NonSerialized]
	public bool disableRecast;

	[NonSerialized]
	public bool disableRoot;

	[NonSerialized]
	public float explosionDamageMultiplier = 1f;

	[NonSerialized]
	[SyncVar]
	public float explosionScaleMultiplier = 1f;

	[NonSerialized]
	public bool isDashMode;

	internal Vector3 _projectilePos;

	private AbilityTrigger.ChangedConfigHandle _handle;

	private Vector3 _baseExplodeEffectScale;

	private Vector3 _baseExplodeRangeScale;

	public override bool reuseInRoom => true;

	public float NetworkexplosionScaleMultiplier
	{
		get
		{
			return explosionScaleMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref explosionScaleMultiplier, 64uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_baseExplodeEffectScale = explodeEffect.transform.localScale;
		_baseExplodeRangeScale = explodeRange.transform.localScale;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		NetworkexplosionScaleMultiplier = 1f;
		explosionDamageMultiplier = 1f;
		disableRoot = false;
		disableRecast = false;
		isDashMode = false;
	}

	protected override void OnCreate()
	{
		explodeEffect.transform.localScale = _baseExplodeEffectScale * explosionScaleMultiplier;
		explodeRange.transform.localScale = _baseExplodeRangeScale * explosionScaleMultiplier;
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (!isDashMode)
		{
			CreateAbilityInstance<Ai_Q_EtherealInfluence_Projectile_Forward>(info.caster.position, Quaternion.identity, new CastInfo(info.caster, info.angle));
		}
		if (isDashMode && !disableRecast)
		{
			FxPlayNetworked(fxDashMode, info.caster);
		}
		if ((UnityEngine.Object)(object)firstTrigger != null && !disableRecast)
		{
			float duration = (isDashMode ? 1.5f : 10f);
			_handle = firstTrigger.ChangeConfigTimedOnce(1, duration, OnUse, () =>
			{
				if (isActive)
				{
					if (isDashMode)
					{
						_projectilePos = info.caster.position;
						FxStopNetworked(fxDashMode);
						Explode();
					}
					Destroy();
				}
			}, setFillAmount: false);
		}
		ActorEvent_OnAbilityInstanceCreated += (Action<EventInfoAbilityInstance>)((EventInfoAbilityInstance obj) =>
		{
			if (obj.instance is Ai_Q_EtherealInfluence_Projectile_Backward)
			{
				obj.instance.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
				{
					if (_handle != null && _handle.isActive)
					{
						_handle.Stop();
						_handle = null;
						OnUse(default);
					}
					if (isActive)
					{
						Destroy();
					}
				});
			}
		});
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && Time.time - creationTime > 20f)
		{
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			FxStopNetworked(fxDashMode);
			if (_handle != null && _handle.isActive)
			{
				_handle.Stop();
			}
		}
	}

	private void OnUse(EventInfoAbilityInstance obj)
	{
		if (isDashMode)
		{
			_projectilePos = info.caster.position;
		}
		_handle = null;
		Explode();
	}

	public void Explode(float damageRatio = 1f)
	{
		FxStopNetworked(fxDashMode);
		FxPlayNewNetworked(explodeEffect, _projectilePos, Quaternion.identity);
		explodeRange.transform.position = _projectilePos;
		List<Entity> entities = explodeRange.GetEntities(out var handle, tvDefaultHarmfulEffectTargets);
		foreach (Entity item in entities)
		{
			DamageData damageData = Damage(explodeDamage, explodeProcCoefficient).SetElemental(ElementalType.Light).ApplyRawMultiplier(explosionDamageMultiplier).ApplyRawMultiplier(damageRatio)
				.SetOriginPosition(_projectilePos);
			if (entities.Count == 1)
			{
				damageData.ApplyAmplification(explodeAmpOnSingleTarget);
				damageData.SetAttr(DamageAttribute.IsCrit);
			}
			damageData.Dispatch(item);
			FxPlayNewNetworked(explodeHitEffect, item);
			if (!disableRoot)
			{
				if (item.Status.TryGetStatusEffect<Se_Q_EtherealInfluence_Root>(out var effect))
				{
					effect.Destroy();
				}
				CreateStatusEffect<Se_Q_EtherealInfluence_Root>(item);
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
			NetworkWriterExtensions.WriteFloat(writer, explosionScaleMultiplier);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, explosionScaleMultiplier);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref explosionScaleMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref explosionScaleMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
