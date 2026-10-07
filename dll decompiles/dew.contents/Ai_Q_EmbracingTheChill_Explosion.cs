using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Q_EmbracingTheChill_Explosion : InstantDamageInstance
{
	public float stunDuration = 1.5f;

	[NonSerialized]
	[SyncVar]
	public float sizeMultiplier = 1f;

	public bool isSelfDamage;

	public float takeDamageRatio;

	public float NetworksizeMultiplier
	{
		get
		{
			return sizeMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref sizeMultiplier, 128uL, (Action<float, float>)null);
		}
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		if (isSelfDamage)
		{
			hittable.targets |= EntityRelation.Self;
		}
	}

	protected override void OnCreate()
	{
		((Component)(object)this).transform.localScale *= sizeMultiplier;
		base.OnCreate();
	}

	protected override void OnHit(Entity entity)
	{
		if (isSelfDamage && (UnityEngine.Object)(object)entity == (UnityEngine.Object)(object)info.caster)
		{
			Damage(dmgFactor).SetElemental(ElementalType.Cold).ApplyReduction(1f - takeDamageRatio).Dispatch(entity);
			return;
		}
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration);
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
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
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
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sizeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
