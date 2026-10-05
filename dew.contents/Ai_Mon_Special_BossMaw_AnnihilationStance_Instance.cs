using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossMaw_AnnihilationStance_Instance : StandardProjectile
{
	[NonSerialized]
	[SyncVar]
	public bool isLeft;

	public Transform invertedTransform;

	public ScalingValue damage;

	public bool NetworkisLeft
	{
		get
		{
			return isLeft;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isLeft, 524288uL, (Action<bool, bool>)null);
		}
	}

	protected override void OnCreate()
	{
		if (isLeft && invertedTransform != null)
		{
			invertedTransform.localScale = invertedTransform.localScale.WithX(0f - invertedTransform.localScale.x);
		}
		base.OnCreate();
	}

	protected override void OnEntity(EntityHit hit)
	{
		if (!(Vector3.Dot(info.forward, hit.entity.position - info.caster.position) < 0f))
		{
			base.OnEntity(hit);
			CreateDamage(DamageData.SourceType.Default, damage).SetElemental(ElementalType.Dark).SetDirection(info.forward).Dispatch(hit.entity);
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
			NetworkWriterExtensions.WriteBool(writer, isLeft);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isLeft);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isLeft, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isLeft, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
