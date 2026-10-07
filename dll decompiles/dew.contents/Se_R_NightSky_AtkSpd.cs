using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_R_NightSky_AtkSpd : StatusEffect
{
	public AnimationCurve scaleCurve;

	public float scaleSpeed;

	[SyncVar]
	internal float _effectStrength;

	public HasteEffect haste { get; private set; } = new HasteEffect();

	public override bool reuseInRoom => true;

	public float Network_effectStrength
	{
		get
		{
			return _effectStrength;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref _effectStrength, 4096uL, (Action<float, float>)null);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		haste.strength = 0f;
		Network_effectStrength = 0f;
		if (startEffectVictim != null)
		{
			startEffectVictim.transform.localScale = Vector3.zero;
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoBasicEffect(haste);
			startEffectVictim.transform.localScale = Vector3.zero;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		float x = startEffectVictim.transform.localScale.x;
		float target = scaleCurve.Evaluate(_effectStrength);
		startEffectVictim.transform.localScale = Vector3.one * Mathf.MoveTowards(x, target, scaleSpeed * dt);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloat(writer, _effectStrength);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, _effectStrength);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _effectStrength, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref _effectStrength, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
