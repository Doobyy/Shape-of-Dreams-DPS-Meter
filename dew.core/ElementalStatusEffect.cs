using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class ElementalStatusEffect : StackedStatusEffect, IOtherPlayersTonedDownDisable
{
	[NonSerialized]
	[SyncVar]
	public float ampAmount;

	public ParticleSystem[] stackScaledEmissions;

	private EmissionModule[] _stackScaledEmissions;

	private float[] _originalROD;

	private float[] _originalROT;

	public float NetworkampAmount
	{
		get
		{
			return ampAmount;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref ampAmount, 16384uL, (Action<float, float>)null);
		}
	}

	protected override void Awake()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		if (stackScaledEmissions != null)
		{
			_stackScaledEmissions = new EmissionModule[stackScaledEmissions.Length];
			_originalROD = new float[stackScaledEmissions.Length];
			_originalROT = new float[stackScaledEmissions.Length];
			for (int i = 0; i < stackScaledEmissions.Length; i++)
			{
				ParticleSystem val = stackScaledEmissions[i];
				_stackScaledEmissions[i] = val.emission;
				_originalROD[i] = _stackScaledEmissions[i].rateOverDistanceMultiplier;
				_originalROT[i] = _stackScaledEmissions[i].rateOverTimeMultiplier;
			}
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		UpdateStackScaledEmissions();
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		UpdateStackScaledEmissions();
	}

	private void UpdateStackScaledEmissions()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (stackScaledEmissions != null)
		{
			for (int i = 0; i < _stackScaledEmissions.Length; i++)
			{
				EmissionModule val = _stackScaledEmissions[i];
				val.rateOverDistanceMultiplier = _originalROD[i] * (float)stack;
				val.rateOverTimeMultiplier = _originalROT[i] * (float)stack;
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
			NetworkWriterExtensions.WriteFloat(writer, ampAmount);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x4000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, ampAmount);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref ampAmount, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x4000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref ampAmount, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
