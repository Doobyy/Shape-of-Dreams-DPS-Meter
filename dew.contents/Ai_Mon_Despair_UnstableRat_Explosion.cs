using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Despair_UnstableRat_Explosion : InstantDamageInstance
{
	public bool disableRenderers = true;

	public Vector2 delayMultiplierRange;

	[SyncVar]
	private float _delayMultiplier;

	private bool _baseDamageDelayCaptured;

	private float _baseDamageDelay;

	public override bool reuseInRoom => true;

	public float Network_delayMultiplier
	{
		get
		{
			return _delayMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _delayMultiplier, 128uL, (Action<float, float>)null);
		}
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		Network_delayMultiplier = UnityEngine.Random.Range(delayMultiplierRange.x, delayMultiplierRange.y);
	}

	protected override void OnCreate()
	{
		if (!_baseDamageDelayCaptured)
		{
			_baseDamageDelay = damageDelay;
			_baseDamageDelayCaptured = true;
		}
		damageDelay = _baseDamageDelay * _delayMultiplier;
		base.OnCreate();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (disableRenderers && (UnityEngine.Object)(object)info.caster != null)
		{
			info.caster.Visual.DisableRenderersLocal();
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
			NetworkWriterExtensions.WriteFloat(writer, _delayMultiplier);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _delayMultiplier);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _delayMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _delayMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
