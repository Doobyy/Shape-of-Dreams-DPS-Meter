using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class St_D_IceColdPresence : SkillTrigger
{
	[CompilerGenerated]
	[SyncVar]
	private float gainedHealth__BackingField;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public string originalSkill;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public int maxCharges;

	private float _ensureEffectTimer;

	public float baseCooldownTime => firstTrigger.currentConfigMaxCooldownTime;

	private float gainedHealth
	{
		[CompilerGenerated]
		get
		{
			return gainedHealth__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CgainedHealth_003Ek__BackingField = value;
		}
	}

	public float Network_003CgainedHealth_003Ek__BackingField
	{
		get
		{
			return gainedHealth__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref gainedHealth__BackingField, 134217728uL, (Action<float, float>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_ensureEffectTimer = 0f;
			ClientTriggerEvent_OnCurrentConfigCharged += new Action(EnsureEffect);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer)
		{
			_ensureEffectTimer += dt;
			if (!(_ensureEffectTimer < 1f))
			{
				_ensureEffectTimer = 0f;
				EnsureEffect();
			}
		}
	}

	private void EnsureEffect()
	{
		if (!((UnityEngine.Object)(object)owner == null) && currentConfigCurrentCharge > 0 && !owner.Status.HasStatusEffect<Se_D_IceColdPresence>() && (!((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.softInstance != null) || !NetworkedManagerBase<ZoneManager>.softInstance.isInAnyTransition))
		{
			CreateStatusEffect<Se_D_IceColdPresence>(owner, new CastInfo(owner));
		}
	}

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Dew.CallDelayed(() =>
		{
			if (!((UnityEngine.Object)(object)newOwner == null))
			{
				if (newOwner.Status.TryGetStatusEffect<Se_D_IceColdPresence_PersistentBuff>(out var effect))
				{
					Network_003CgainedHealth_003Ek__BackingField = effect.bonus.maxHealthFlat;
				}
				if (currentConfigCurrentCharge > 0 && (UnityEngine.Object)(object)newOwner != null && !newOwner.Status.HasStatusEffect<Se_D_IceColdPresence>())
				{
					CreateStatusEffect<Se_D_IceColdPresence>(newOwner, new CastInfo(newOwner));
				}
			}
		});
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)formerOwner == null) && formerOwner.Status.TryGetStatusEffect<Se_D_IceColdPresence>(out var effect))
		{
			effect.Destroy();
		}
	}

	public void UpdateStack(float maxHealthFlat)
	{
		Network_003CgainedHealth_003Ek__BackingField = maxHealthFlat;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, gainedHealth__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, gainedHealth__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref gainedHealth__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x8000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref gainedHealth__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
