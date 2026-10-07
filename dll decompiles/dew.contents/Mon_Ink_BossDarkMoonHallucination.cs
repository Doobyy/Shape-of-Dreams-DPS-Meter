using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Mon_Ink_BossDarkMoonHallucination : Monster
{
	[SyncVar]
	internal bool _isSpecialAtk;

	public bool Network_isSpecialAtk
	{
		get
		{
			return _isSpecialAtk;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isSpecialAtk, 256uL, (Action<bool, bool>)null);
		}
	}

	public override void OnModelLoaded()
	{
		base.OnModelLoaded();
		if (_isSpecialAtk)
		{
			Visual.model.GetCustomMapping<GameObject>("blade").SetActive(value: false);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (((NetworkBehaviour)this).isServer)
		{
			Network_isSpecialAtk = false;
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(this, new InvisibleEffect
			{
				ignoreReveal = true
			}, float.PositiveInfinity);
			CreateBasicEffect(this, new UntargetableEffect(), float.PositiveInfinity);
			CreateBasicEffect(this, new InvulnerableEffect(), float.PositiveInfinity);
			CreateBasicEffect(this, new UncollidableEffect(), float.PositiveInfinity);
			Visual.HideGroundMarker();
			if (!_isSpecialAtk)
			{
				CreateAbilityInstance<Ai_Mon_Ink_BossDarkMoon_Blade_RageInstance>(position, rotation, new CastInfo(this, CastInfo.GetAngle(((Component)(object)this).transform.forward)));
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
			NetworkWriterExtensions.WriteBool(writer, _isSpecialAtk);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isSpecialAtk);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isSpecialAtk, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isSpecialAtk, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
