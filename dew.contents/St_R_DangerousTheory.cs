using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class St_R_DangerousTheory : SkillTrigger
{
	public float healHpThreshold = 0.6f;

	[SyncVar]
	public bool preventCastWhenNotEmpowered;

	public bool NetworkpreventCastWhenNotEmpowered
	{
		get
		{
			return preventCastWhenNotEmpowered;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref preventCastWhenNotEmpowered, 134217728uL, (Action<bool, bool>)null);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)owner == null))
		{
			fillAmount = ((CanBeReserved() && IsEmpowered()) ? 1f : 0f);
		}
	}

	private bool IsEmpowered()
	{
		return owner.currentHealth / owner.maxHealth < healHpThreshold;
	}

	public override bool CanBeReserved()
	{
		if (base.CanBeReserved())
		{
			if (preventCastWhenNotEmpowered)
			{
				return IsEmpowered();
			}
			return true;
		}
		return false;
	}

	public override bool CanBeCast()
	{
		if (base.CanBeCast())
		{
			if (preventCastWhenNotEmpowered)
			{
				return IsEmpowered();
			}
			return true;
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, preventCastWhenNotEmpowered);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, preventCastWhenNotEmpowered);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref preventCastWhenNotEmpowered, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x8000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref preventCastWhenNotEmpowered, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
