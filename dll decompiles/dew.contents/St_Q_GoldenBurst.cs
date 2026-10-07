using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class St_Q_GoldenBurst : SkillTrigger
{
	public float perStackAmp = 0.2f;

	[NonSerialized]
	[SyncVar]
	public float sacrificeHpMultiplier = 1f;

	[NonSerialized]
	[SyncVar]
	public bool castForFree;

	[NonSerialized]
	[SyncVar]
	public bool castForFreeIfNoWitherInCombat;

	private Ai_Q_GoldenBurst _goldenBurstPrefab;

	public float NetworksacrificeHpMultiplier
	{
		get
		{
			return sacrificeHpMultiplier;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref sacrificeHpMultiplier, 134217728uL, (Action<float, float>)null);
		}
	}

	public bool NetworkcastForFree
	{
		get
		{
			return castForFree;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref castForFree, 268435456uL, (Action<bool, bool>)null);
		}
	}

	public bool NetworkcastForFreeIfNoWitherInCombat
	{
		get
		{
			return castForFreeIfNoWitherInCombat;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref castForFreeIfNoWitherInCombat, 536870912uL, (Action<bool, bool>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		dealtDamageProcessor.Add(delegate(ref DamageData data, Actor actor, Entity target)
		{
			if (actor is Ai_Q_GoldenBurst && !((UnityEngine.Object)(object)owner == null) && owner.CheckEnemyOrNeutral(target) && !data.IsAmountModifiedBy(this) && owner.Status.TryGetStatusEffect<Se_Q_GoldenBurst_Wither>(out var effect) && effect.stack > 0)
			{
				data.SetAttr(DamageAttribute.IsCrit);
				data.ApplyAmplification(perStackAmp * (float)effect.stack);
				data.SetAmountModifiedBy(this);
			}
		});
		dealtHealProcessor.Add(delegate(ref HealData data, Actor actor, Entity target)
		{
			if (actor is Ai_Q_GoldenBurst && !((UnityEngine.Object)(object)owner == null) && !data.IsAmountModifiedBy(this) && owner.Status.TryGetStatusEffect<Se_Q_GoldenBurst_Wither>(out var effect) && effect.stack > 0)
			{
				data.SetCrit();
				data.ApplyAmplification(perStackAmp * (float)effect.stack);
				data.SetAmountModifiedBy(this);
			}
		});
	}

	public override bool CanBeReserved()
	{
		if ((UnityEngine.Object)(object)_goldenBurstPrefab == null)
		{
			_goldenBurstPrefab = DewResources.GetByType<Ai_Q_GoldenBurst>(default(ResourceLoadSettings));
		}
		if (base.CanBeReserved() && (UnityEngine.Object)(object)owner != null)
		{
			if (!castForFree)
			{
				return _goldenBurstPrefab.GetSelfDamageAmount(owner) * sacrificeHpMultiplier < owner.currentHealth;
			}
			return true;
		}
		return false;
	}

	public override bool CanBeCast()
	{
		if ((UnityEngine.Object)(object)_goldenBurstPrefab == null)
		{
			_goldenBurstPrefab = DewResources.GetByType<Ai_Q_GoldenBurst>(default(ResourceLoadSettings));
		}
		if (base.CanBeCast() && (UnityEngine.Object)(object)owner != null)
		{
			if (!castForFree)
			{
				return _goldenBurstPrefab.GetSelfDamageAmount(owner) * sacrificeHpMultiplier < owner.currentHealth;
			}
			return true;
		}
		return false;
	}

	public override void OnCastCompleteBeforePrepare(EventInfoCast cast)
	{
		base.OnCastCompleteBeforePrepare(cast);
		int num = (owner.Status.TryGetStatusEffect<Se_Q_GoldenBurst_Wither>(out var effect) ? effect.stack : 0);
		if (cast.instance is Ai_Q_GoldenBurst ai_Q_GoldenBurst)
		{
			ai_Q_GoldenBurst.isCastForFree = castForFree || (castForFreeIfNoWitherInCombat && owner.isInCombat && num == 0);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)owner != null)
		{
			if ((UnityEngine.Object)(object)_goldenBurstPrefab == null)
			{
				_goldenBurstPrefab = DewResources.GetByType<Ai_Q_GoldenBurst>(default(ResourceLoadSettings));
			}
			float num = _goldenBurstPrefab.GetSelfDamageAmount(owner) * sacrificeHpMultiplier;
			owner.Status.specialFill = new Vector2(owner.currentHealth - num, owner.currentHealth);
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
			NetworkWriterExtensions.WriteFloat(writer, sacrificeHpMultiplier);
			NetworkWriterExtensions.WriteBool(writer, castForFree);
			NetworkWriterExtensions.WriteBool(writer, castForFreeIfNoWitherInCombat);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, sacrificeHpMultiplier);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10000000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, castForFree);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20000000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, castForFreeIfNoWitherInCombat);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sacrificeHpMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref castForFree, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref castForFreeIfNoWitherInCombat, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x8000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref sacrificeHpMultiplier, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x10000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref castForFree, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x20000000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref castForFreeIfNoWitherInCombat, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
