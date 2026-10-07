using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_GenericShield_Stacking : StatusEffect
{
	public GameObject fxEffect;

	[NonSerialized]
	public float timeout = 3f;

	[SyncVar(hook = "OnShouldShowEffectChanged")]
	private bool _shouldShowEffect;

	private float _nextExpirationTime;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate__shouldShowEffect;

	public ShieldEffect shield { get; private set; }

	public bool Network_shouldShowEffect
	{
		get
		{
			return _shouldShowEffect;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _shouldShowEffect, 4096uL, _Mirror_SyncVarHookDelegate__shouldShowEffect);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			shield = DoShield(0f);
			ShieldEffect shieldEffect = shield;
			shieldEffect.onAmountModified = (Action<float, float>)Delegate.Combine(shieldEffect.onAmountModified, new Action<float, float>(OnAmountModified));
			_nextExpirationTime = Time.time + timeout;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && Time.time > _nextExpirationTime)
		{
			_nextExpirationTime = float.PositiveInfinity;
			shield.amount = 0f;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			UpdateShowStatus();
		}
	}

	private void OnAmountModified(float arg1, float arg2)
	{
		UpdateShowStatus();
	}

	private void OnShouldShowEffectChanged(bool from, bool to)
	{
		if (to)
		{
			FxPlay(fxEffect, victim);
		}
		else
		{
			FxStop(fxEffect);
		}
	}

	[Server]
	private void UpdateShowStatus()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_GenericShield_Stacking::UpdateShowStatus()' called when server was not active");
		}
		else
		{
			Network_shouldShowEffect = isActive && shield.amount > 0.0001f;
		}
	}

	[Server]
	public void AddAmount(float amount, ReactionChain chain = default(ReactionChain))
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_GenericShield_Stacking::AddAmount(System.Single,ReactionChain)' called when server was not active");
			return;
		}
		shield.AddAmount(amount, float.PositiveInfinity, processTotalMax: true, chain);
		_nextExpirationTime = Time.time + timeout;
	}

	public Se_GenericShield_Stacking()
	{
		_Mirror_SyncVarHookDelegate__shouldShowEffect = OnShouldShowEffectChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, _shouldShowEffect);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _shouldShowEffect);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _shouldShowEffect, _Mirror_SyncVarHookDelegate__shouldShowEffect, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _shouldShowEffect, _Mirror_SyncVarHookDelegate__shouldShowEffect, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
