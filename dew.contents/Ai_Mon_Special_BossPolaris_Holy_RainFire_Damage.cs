using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Mon_Special_BossPolaris_Holy_RainFire_Damage : InstantDamageInstance
{
	[NonSerialized]
	[SyncVar]
	public float scaleMultiplier = 1f;

	[NonSerialized]
	[SyncVar]
	public float angularSpeed;

	private Vector3 _baseLocalScale;

	private bool _cachedBaseScale;

	public float NetworkscaleMultiplier
	{
		get
		{
			return scaleMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref scaleMultiplier, 128uL, (Action<float, float>)null);
		}
	}

	public float NetworkangularSpeed
	{
		get
		{
			return angularSpeed;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref angularSpeed, 256uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (!_cachedBaseScale)
		{
			_baseLocalScale = ((Component)(object)this).transform.localScale;
			_cachedBaseScale = true;
		}
	}

	protected override void OnCreate()
	{
		((Component)(object)this).transform.localScale = _baseLocalScale * scaleMultiplier;
		base.OnCreate();
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		Vector3 agentPosition = info.caster.agentPosition;
		float y = Quaternion.LookRotation(position - agentPosition).eulerAngles.y;
		float num = Vector3.Distance(position, agentPosition);
		position = agentPosition + Quaternion.Euler(0f, y + angularSpeed * Time.deltaTime, 0f) * Vector3.forward * num;
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (!entity.Status.hasDamageImmunity)
		{
			if (entity.Status.TryGetStatusEffect<Se_Mon_Special_BossPolaris_CleansingFlame>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_Mon_Special_BossPolaris_CleansingFlame>(entity);
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
			NetworkWriterExtensions.WriteFloat(writer, scaleMultiplier);
			NetworkWriterExtensions.WriteFloat(writer, angularSpeed);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, scaleMultiplier);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, angularSpeed);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref scaleMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref angularSpeed, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref scaleMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref angularSpeed, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
