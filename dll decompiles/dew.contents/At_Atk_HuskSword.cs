using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class At_Atk_HuskSword : AttackTrigger
{
	[CompilerGenerated]
	[SyncVar]
	private float? cooldownTimeMultiplier__BackingField;

	public DewAnimationClip overrideEndAnim;

	public float? cooldownTimeMultiplier
	{
		[CompilerGenerated]
		get
		{
			return cooldownTimeMultiplier__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcooldownTimeMultiplier_003Ek__BackingField = value;
		}
	}

	public float? Network_003CcooldownTimeMultiplier_003Ek__BackingField
	{
		get
		{
			return cooldownTimeMultiplier__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float?>(value, ref cooldownTimeMultiplier__BackingField, 1024uL, (Action<float?, float?>)null);
		}
	}

	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		if (ignoreRangeCheck && (UnityEngine.Object)(object)info.target != null)
		{
			info.angle = CastInfo.GetAngle(info.target.position - owner.position);
			info.target = null;
		}
		AbilityInstance result = base.OnCastComplete(configIndex, info);
		if (overrideEndAnim != null)
		{
			owner.Animation.PlayAbilityAnimation(overrideEndAnim);
		}
		return result;
	}

	public override float GetCooldownTimeMultiplier(int configIndex)
	{
		return base.GetCooldownTimeMultiplier(configIndex) * (cooldownTimeMultiplier ?? 1f);
	}

	public override float GetAnimationSpeed()
	{
		return 1f / GetCooldownTimeMultiplier(0);
	}

	public override float GetChannelDurationMultiplier()
	{
		return GetCooldownTimeMultiplier(0);
	}

	public override float GetPostDelayDurationMultiplier()
	{
		return GetCooldownTimeMultiplier(0);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteFloatNullable(writer, cooldownTimeMultiplier__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteFloatNullable(writer, cooldownTimeMultiplier__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float?>(ref cooldownTimeMultiplier__BackingField, (Action<float?, float?>)null, NetworkReaderExtensions.ReadFloatNullable(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float?>(ref cooldownTimeMultiplier__BackingField, (Action<float?, float?>)null, NetworkReaderExtensions.ReadFloatNullable(reader));
		}
	}
}
