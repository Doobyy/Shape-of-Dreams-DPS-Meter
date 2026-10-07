using System;
using System.Runtime.InteropServices;
using Mirror;

public class Ai_Mon_Special_BossPolaris_Holy_Atk_Projectile : StandardProjectile
{
	[NonSerialized]
	[SyncVar]
	public float sizeMultiplier = 1f;

	public float NetworksizeMultiplier
	{
		get
		{
			return sizeMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref sizeMultiplier, 524288uL, (Action<float, float>)null);
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		CreateAbilityInstance(info.point, null, new CastInfo(info.caster), (Ai_Mon_Special_BossPolaris_Holy_Atk_Explosion ai) =>
		{
			ai.NetworksizeMultiplier = sizeMultiplier;
		});
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, sizeMultiplier);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, sizeMultiplier);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sizeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sizeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
