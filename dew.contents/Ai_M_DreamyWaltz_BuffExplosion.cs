using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_M_DreamyWaltz_BuffExplosion : AbilityInstance
{
	public DewCollider range;

	[NonSerialized]
	[SyncVar]
	public float sizeMultiplier = 1f;

	[NonSerialized]
	public float selfMultiplier;

	private Vector3 _baseRangeScale;

	private Vector3 _baseStartEffectScale;

	private Vector3 _baseStartEffectNoStopScale;

	public override bool reuseInRoom => true;

	public float NetworksizeMultiplier
	{
		get
		{
			return sizeMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref sizeMultiplier, 64uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (range != null)
		{
			_baseRangeScale = range.transform.localScale;
		}
		if (startEffect != null)
		{
			_baseStartEffectScale = startEffect.transform.localScale;
		}
		if (startEffectNoStop != null)
		{
			_baseStartEffectNoStopScale = startEffectNoStop.transform.localScale;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (range != null)
		{
			range.transform.localScale = _baseRangeScale;
		}
		if (startEffect != null)
		{
			startEffect.transform.localScale = _baseStartEffectScale;
		}
		if (startEffectNoStop != null)
		{
			startEffectNoStop.transform.localScale = _baseStartEffectNoStopScale;
		}
		NetworksizeMultiplier = 1f;
		selfMultiplier = 0f;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		range.transform.localScale *= sizeMultiplier;
		if (startEffect != null)
		{
			startEffect.transform.localScale *= sizeMultiplier;
		}
		if (startEffectNoStop != null)
		{
			startEffectNoStop.transform.localScale *= sizeMultiplier;
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ListReturnHandle<Entity> handle;
		foreach (Entity e in range.GetEntities(out handle, tvDefaultUsefulEffectTargets, new CollisionCheckSettings
		{
			includeUncollidable = true
		}))
		{
			if ((double)selfMultiplier <= 0.001 && (UnityEngine.Object)(object)e == (UnityEngine.Object)(object)info.caster)
			{
				continue;
			}
			if (e.Status.TryGetStatusEffect<Se_M_DreamyWaltz_Buff>(out var effect))
			{
				effect.Destroy();
			}
			CreateStatusEffect(e, (Se_M_DreamyWaltz_Buff se) =>
			{
				if ((UnityEngine.Object)(object)e == (UnityEngine.Object)(object)info.caster)
				{
					se.strengthMultiplier = selfMultiplier;
				}
			});
		}
		handle.Return();
		Destroy();
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
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
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
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sizeMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
