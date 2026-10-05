using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Gem_L_MetalCrystal : Gem, IIgnoreFirstGemUpgrade
{
	[NonSerialized]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public float healthBonusPercentage;

	private StatBonus _bonus;

	public float NetworkhealthBonusPercentage
	{
		get
		{
			return healthBonusPercentage;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref healthBonusPercentage, 262144uL, (Action<float, float>)null);
		}
	}

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer && newOwner is Hero_Bismuth)
		{
			_bonus = newOwner.Status.AddStatBonus(new StatBonus
			{
				maxHealthPercentage = healthBonusPercentage
			});
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && _bonus != null)
		{
			if ((UnityEngine.Object)(object)oldOwner != null)
			{
				oldOwner.Status.RemoveStatBonus(_bonus);
			}
			_bonus = null;
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
			NetworkWriterExtensions.WriteFloat(writer, healthBonusPercentage);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, healthBonusPercentage);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref healthBonusPercentage, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref healthBonusPercentage, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
	}
}
