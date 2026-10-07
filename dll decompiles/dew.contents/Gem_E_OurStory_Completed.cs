using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Gem_E_OurStory_Completed : Gem
{
	[NonSerialized]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public float statPercentage;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	public float dodgeEffectAmp;

	private StatBonus _bonus;

	public float NetworkstatPercentage
	{
		get
		{
			return statPercentage;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref statPercentage, 262144uL, (Action<float, float>)null);
		}
	}

	public float NetworkdodgeEffectAmp
	{
		get
		{
			return dodgeEffectAmp;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref dodgeEffectAmp, 524288uL, (Action<float, float>)null);
		}
	}

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = newOwner.Status.AddStatBonus(new StatBonus
			{
				abilityPowerPercentage = statPercentage,
				attackDamagePercentage = statPercentage
			});
			newOwner.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_M_Sprint se_M_Sprint)
		{
			se_M_Sprint.hasteAmount *= 1f + dodgeEffectAmp;
			se_M_Sprint.speedAmount *= 1f + dodgeEffectAmp;
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (_bonus != null)
		{
			if ((UnityEngine.Object)(object)oldOwner != null)
			{
				oldOwner.Status.RemoveStatBonus(_bonus);
			}
			_bonus = null;
		}
		if ((UnityEngine.Object)(object)oldOwner != null)
		{
			oldOwner.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !owner.IsNullInactiveDeadOrKnockedOut() && !(owner is Hero_Bismuth) && UnityEngine.Random.value < 0.1f)
		{
			PureDamage(owner.maxHealth * 0.015f).SetAttr(DamageAttribute.DamageOverTime).Dispatch(owner);
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
			NetworkWriterExtensions.WriteFloat(writer, statPercentage);
			NetworkWriterExtensions.WriteFloat(writer, dodgeEffectAmp);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, statPercentage);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, dodgeEffectAmp);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref statPercentage, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref dodgeEffectAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref statPercentage, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref dodgeEffectAmp, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
