using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Gem_E_Crimson : InstantDamageInstance
{
	public float bossHealAmp;

	public ScalingValue healPerHit;

	public Transform[] scaledTransforms;

	public Transform[] scaledTransformsOnLocal;

	[NonSerialized]
	[SyncVar]
	public float sizeMultiplier;

	private Vector3[] _originalScales;

	private Vector3[] _originalScalesOnLocal;

	private bool _scalesCaptured;

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
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref sizeMultiplier, 128uL, (Action<float, float>)null);
		}
	}

	protected override void OnCreate()
	{
		if (!_scalesCaptured)
		{
			_originalScales = new Vector3[scaledTransforms.Length];
			for (int i = 0; i < scaledTransforms.Length; i++)
			{
				if (!(scaledTransforms[i] == null))
				{
					_originalScales[i] = scaledTransforms[i].localScale;
				}
			}
			_originalScalesOnLocal = new Vector3[scaledTransformsOnLocal.Length];
			for (int j = 0; j < scaledTransformsOnLocal.Length; j++)
			{
				if (!(scaledTransformsOnLocal[j] == null))
				{
					_originalScalesOnLocal[j] = scaledTransformsOnLocal[j].localScale;
				}
			}
			_scalesCaptured = true;
		}
		for (int k = 0; k < scaledTransforms.Length; k++)
		{
			Transform transform = scaledTransforms[k];
			if (!(transform == null))
			{
				transform.localScale = _originalScales[k] * sizeMultiplier;
			}
		}
		for (int l = 0; l < scaledTransformsOnLocal.Length; l++)
		{
			Transform transform2 = scaledTransformsOnLocal[l];
			if (!(transform2 == null))
			{
				transform2.localScale = _originalScalesOnLocal[l] * sizeMultiplier;
			}
		}
		base.OnCreate();
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		if (!entity.Status.hasDamageImmunity)
		{
			HealData healData = Heal(healPerHit);
			if (entity.IsAnyBoss())
			{
				healData.SetCrit();
				healData.ApplyAmplification(bossHealAmp);
			}
			healData.Dispatch(info.caster);
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
