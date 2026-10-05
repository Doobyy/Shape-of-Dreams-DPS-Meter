using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public abstract class HealthThresholdBonusStarEffect : StarEffect
{
	public float healthRatioThreshold = 0.5f;

	private float _lastCheckTime;

	[SyncVar(hook = "OnIsLowChanged")]
	private bool _isLowHealth;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__isLowHealth;

	public bool isLowHealth
	{
		get
		{
			if (!Mathf.Approximately(_lastCheckTime, Time.time))
			{
				UpdateStatus(default);
			}
			return _isLowHealth;
		}
	}

	public bool Network_isLowHealth
	{
		get
		{
			return _isLowHealth;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isLowHealth, 8192uL, _Mirror_SyncVarHookDelegate__isLowHealth);
		}
	}

	private void OnIsLowChanged(bool _, bool __)
	{
		OnHealthStateChanged();
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(UpdateStatus);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(UpdateStatus);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && Time.time - _lastCheckTime > 0.5f)
		{
			UpdateStatus(default);
		}
	}

	public virtual void OnHealthStateChanged()
	{
	}

	private void UpdateStatus(EventInfoDamage _)
	{
		_lastCheckTime = Time.time;
		Network_isLowHealth = hero.normalizedHealth < healthRatioThreshold;
	}

	protected HealthThresholdBonusStarEffect()
	{
		_Mirror_SyncVarHookDelegate__isLowHealth = OnIsLowChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, _isLowHealth);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isLowHealth);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isLowHealth, _Mirror_SyncVarHookDelegate__isLowHealth, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isLowHealth, _Mirror_SyncVarHookDelegate__isLowHealth, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
