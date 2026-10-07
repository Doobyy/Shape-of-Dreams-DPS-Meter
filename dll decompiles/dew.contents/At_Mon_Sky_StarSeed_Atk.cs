using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class At_Mon_Sky_StarSeed_Atk : AttackTrigger
{
	[NonSerialized]
	[SyncVar(hook = "OnIsMegaExplosionChanged")]
	public bool isMegaExplosion;

	public GameObject megaExplosionTelegraphObject;

	public Action<bool, bool> _Mirror_SyncVarHookDelegate_isMegaExplosion;

	public bool NetworkisMegaExplosion
	{
		get
		{
			return isMegaExplosion;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isMegaExplosion, 1024uL, _Mirror_SyncVarHookDelegate_isMegaExplosion);
		}
	}

	private void OnIsMegaExplosionChanged(bool from, bool to)
	{
		megaExplosionTelegraphObject.SetActive(to);
		ListReturnHandle<DewAudioSource> handle;
		foreach (DewAudioSource item in configs[0].effectOnCast.GetComponentsInChildrenNonAlloc(out handle))
		{
			if (to)
			{
				item.pitchMultiplier *= 0.7f;
			}
			else
			{
				item.pitchMultiplier /= 0.7f;
			}
		}
		handle.Return();
		InvalidateInstance();
	}

	public override void OnCastCompleteBeforePrepare(EventInfoCast cast)
	{
		base.OnCastCompleteBeforePrepare(cast);
		((Ai_Mon_Sky_StarSeed_Atk)cast.instance).NetworkisMegaExplosion = isMegaExplosion;
	}

	public At_Mon_Sky_StarSeed_Atk()
	{
		_Mirror_SyncVarHookDelegate_isMegaExplosion = OnIsMegaExplosionChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, isMegaExplosion);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isMegaExplosion);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isMegaExplosion, _Mirror_SyncVarHookDelegate_isMegaExplosion, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isMegaExplosion, _Mirror_SyncVarHookDelegate_isMegaExplosion, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
