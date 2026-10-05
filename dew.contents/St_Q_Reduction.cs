using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class St_Q_Reduction : SkillTrigger
{
	[NonSerialized]
	[SyncVar]
	public float? sacrificeHpRatioOverride;

	private Ai_Q_Reduction_Spawner _reductionPrefab;

	public float? NetworksacrificeHpRatioOverride
	{
		get
		{
			return sacrificeHpRatioOverride;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float?>(value, ref sacrificeHpRatioOverride, 134217728uL, (Action<float?, float?>)null);
		}
	}

	public override bool CanBeReserved()
	{
		if ((UnityEngine.Object)(object)_reductionPrefab == null)
		{
			_reductionPrefab = DewResources.GetByType<Ai_Q_Reduction_Spawner>(default(ResourceLoadSettings));
		}
		if (base.CanBeReserved() && (UnityEngine.Object)(object)owner != null)
		{
			return owner.normalizedHealth > _reductionPrefab.sacrificeHpRatio;
		}
		return false;
	}

	public override bool CanBeCast()
	{
		if ((UnityEngine.Object)(object)_reductionPrefab == null)
		{
			_reductionPrefab = DewResources.GetByType<Ai_Q_Reduction_Spawner>(default(ResourceLoadSettings));
		}
		if (base.CanBeCast() && (UnityEngine.Object)(object)owner != null)
		{
			return owner.normalizedHealth > _reductionPrefab.sacrificeHpRatio;
		}
		return false;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)owner != null)
		{
			if ((UnityEngine.Object)(object)_reductionPrefab == null)
			{
				_reductionPrefab = DewResources.GetByType<Ai_Q_Reduction_Spawner>(default(ResourceLoadSettings));
			}
			float num = (sacrificeHpRatioOverride ?? _reductionPrefab.sacrificeHpRatio) * owner.maxHealth;
			owner.Status.specialFill = new Vector2(owner.currentHealth - num, owner.currentHealth);
		}
	}

	public override void OnCastCompleteBeforePrepare(EventInfoCast cast)
	{
		base.OnCastCompleteBeforePrepare(cast);
		if (sacrificeHpRatioOverride.HasValue && cast.instance is Ai_Q_Reduction_Spawner ai_Q_Reduction_Spawner)
		{
			ai_Q_Reduction_Spawner.sacrificeHpRatio = sacrificeHpRatioOverride.Value;
		}
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)formerOwner != null)
		{
			formerOwner.Status.specialFill = Vector2.zero;
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
			NetworkWriterExtensions.WriteFloatNullable(writer, sacrificeHpRatioOverride);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloatNullable(writer, sacrificeHpRatioOverride);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float?>(ref sacrificeHpRatioOverride, (Action<float?, float?>)null, NetworkReaderExtensions.ReadFloatNullable(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x8000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float?>(ref sacrificeHpRatioOverride, (Action<float?, float?>)null, NetworkReaderExtensions.ReadFloatNullable(reader));
		}
	}
}
