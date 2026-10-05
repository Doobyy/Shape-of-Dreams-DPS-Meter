using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_Gem_R_Wound_Wounded : StatusEffect
{
	public float duration;

	public DewCollider explodeRange;

	public ScalingValue hitDamage;

	public ScalingValue explodeDamage;

	public float hitProcCoefficient = 0.5f;

	public float explodeProcCoefficient = 1f;

	public int explodeStage;

	public GameObject[] stageEffect;

	public GameObject hitEffect;

	public GameObject explodeEffect;

	[SyncVar(hook = "OnStageChanged")]
	private int _stage;

	private Action<EventInfoAttackEffect> _ownerAttackedCached;

	public Action<int, int> _Mirror_SyncVarHookDelegate__stage;

	public override bool reuseInRoom => true;

	public int Network_stage
	{
		get
		{
			return _stage;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _stage, 4096uL, _Mirror_SyncVarHookDelegate__stage);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Network_stage = 0;
		ClientActorEvent_OnDestroyed = null;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		OnStageChanged(-1, _stage);
		if (((NetworkBehaviour)this).isServer)
		{
			SetTimer(duration);
			info.caster.EntityEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(OwnerAttacked);
		}
	}

	private void OwnerAttacked(EventInfoAttackEffect obj)
	{
		if (!isActive || _stage >= explodeStage || (UnityEngine.Object)(object)obj.victim != (UnityEngine.Object)(object)victim || info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			return;
		}
		ResetTimer();
		Network_stage = _stage + 1;
		if (_stage >= explodeStage)
		{
			FxPlayNewNetworked(explodeEffect, victim);
			explodeRange.transform.position = victim.position;
			ListReturnHandle<Entity> handle;
			foreach (Entity entity in explodeRange.GetEntities(out handle, tvDefaultHarmfulEffectTargets))
			{
				Damage(explodeDamage, explodeProcCoefficient).ApplyStrength(obj.strength).SetOriginPosition(info.caster.position).SetAttr(DamageAttribute.IsCrit)
					.Dispatch(entity, chain);
			}
			handle.Return();
			DestroyIfActive();
		}
		else
		{
			FxPlayNetworked(hitEffect, victim);
			Damage(hitDamage, hitProcCoefficient).SetOriginPosition(info.caster.position).Dispatch(victim, chain);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_stage < stageEffect.Length)
		{
			FxStop(stageEffect[_stage]);
		}
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)info.caster != null)
		{
			info.caster.EntityEvent_OnAttackEffectTriggered -= new Action<EventInfoAttackEffect>(OwnerAttacked);
		}
	}

	private void OnStageChanged(int oldVal, int newVal)
	{
		if (oldVal >= 0 && oldVal < stageEffect.Length)
		{
			FxStop(stageEffect[oldVal]);
		}
		if (isActive && newVal >= 0 && newVal < stageEffect.Length)
		{
			FxPlay(stageEffect[newVal], victim);
		}
	}

	public Se_Gem_R_Wound_Wounded()
	{
		_Mirror_SyncVarHookDelegate__stage = OnStageChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteInt(writer, _stage);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _stage);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _stage, _Mirror_SyncVarHookDelegate__stage, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _stage, _Mirror_SyncVarHookDelegate__stage, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
