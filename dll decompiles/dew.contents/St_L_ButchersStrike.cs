using System;
using System.Runtime.InteropServices;
using Mirror;

public class St_L_ButchersStrike : SkillTrigger
{
	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	public float currentDamageAmp;

	public float NetworkcurrentDamageAmp
	{
		get
		{
			return currentDamageAmp;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref currentDamageAmp, 134217728uL, (Action<float, float>)null);
		}
	}

	public override void OnCastCompleteBeforePrepare(EventInfoCast cast)
	{
		base.OnCastCompleteBeforePrepare(cast);
		if (cast.instance is Ai_L_ButchersStrike ai_L_ButchersStrike)
		{
			ai_L_ButchersStrike.dmgFactor *= 1f + currentDamageAmp;
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
			NetworkWriterExtensions.WriteFloat(writer, currentDamageAmp);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, currentDamageAmp);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref currentDamageAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x8000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref currentDamageAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
