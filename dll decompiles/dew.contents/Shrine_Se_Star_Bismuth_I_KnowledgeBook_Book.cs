using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Shrine_Se_Star_Bismuth_I_KnowledgeBook_Book : Shrine
{
	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	public string targetPlayerGuid;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	public ChaosRewardType type;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	public int amount;

	public GameObject[] fxLoopByType;

	public GameObject[] fxActivateByType;

	public string NetworktargetPlayerGuid
	{
		get
		{
			return targetPlayerGuid;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<string>(value, ref targetPlayerGuid, 256uL, (Action<string, string>)null);
		}
	}

	public ChaosRewardType Networktype
	{
		get
		{
			return type;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<ChaosRewardType>(value, ref type, 512uL, (Action<ChaosRewardType, ChaosRewardType>)null);
		}
	}

	public int Networkamount
	{
		get
		{
			return amount;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref amount, 1024uL, (Action<int, int>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlay(fxLoopByType[(int)type]);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		for (int i = 0; i < fxLoopByType.Length; i++)
		{
			FxStop(fxLoopByType[i]);
		}
	}

	public override bool CanInteract(Entity entity)
	{
		if (base.CanInteract(entity) && (UnityEngine.Object)(object)entity.owner != null)
		{
			return entity.owner.guid == targetPlayerGuid;
		}
		return false;
	}

	protected override bool OnUse(Entity entity)
	{
		Hero hero = (Hero)entity;
		if (type == ChaosRewardType.MaxHealth)
		{
			Shrine_Chaos.GetBonusStat(hero).maxHealthFlat += amount;
			hero.owner.TpcShowCenterMessage(CenterMessageType.General, "InGame_Message_Chaos_MaxHealth", new string[1] { amount.ToString("#,##0") });
		}
		else if (type == ChaosRewardType.AttackDamage)
		{
			Shrine_Chaos.GetBonusStat(hero).attackDamageFlat += amount;
			hero.owner.TpcShowCenterMessage(CenterMessageType.General, "InGame_Message_Chaos_AttackDamage", new string[1] { amount.ToString("#,##0") });
		}
		else if (type == ChaosRewardType.AttackSpeed)
		{
			Shrine_Chaos.GetBonusStat(hero).attackSpeedPercentage += amount;
			hero.owner.TpcShowCenterMessage(CenterMessageType.General, "InGame_Message_Chaos_AttackSpeed", new string[1] { amount.ToString("#,##0") });
		}
		else if (type == ChaosRewardType.AbilityPower)
		{
			Shrine_Chaos.GetBonusStat(hero).abilityPowerFlat += amount;
			hero.owner.TpcShowCenterMessage(CenterMessageType.General, "InGame_Message_Chaos_AbilityPower", new string[1] { amount.ToString("#,##0") });
		}
		else if (type == ChaosRewardType.AbilityHaste)
		{
			Shrine_Chaos.GetBonusStat(hero).abilityHasteFlat += amount;
			hero.owner.TpcShowCenterMessage(CenterMessageType.General, "InGame_Message_Chaos_AbilityHaste", new string[1] { amount.ToString("#,##0") });
		}
		else if (type == ChaosRewardType.Armor)
		{
			Shrine_Chaos.GetBonusStat(hero).armorFlat += amount;
			hero.owner.TpcShowCenterMessage(CenterMessageType.General, "InGame_Message_Chaos_Armor", new string[1] { amount.ToString("#,##0") });
		}
		if (hero.Status.TryGetStatusEffect<Se_Shrine_Chaos_StatBonus>(out var effect))
		{
			effect.NotifyUpdate();
		}
		FxPlayNetworked(fxActivateByType[(int)type], hero);
		Destroy();
		return true;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteString(writer, targetPlayerGuid);
			GeneratedNetworkCode._Write_ChaosRewardType(writer, type);
			NetworkWriterExtensions.WriteInt(writer, amount);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteString(writer, targetPlayerGuid);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			GeneratedNetworkCode._Write_ChaosRewardType(writer, type);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, amount);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref targetPlayerGuid, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<ChaosRewardType>(ref type, (Action<ChaosRewardType, ChaosRewardType>)null, GeneratedNetworkCode._Read_ChaosRewardType(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref amount, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<string>(ref targetPlayerGuid, (Action<string, string>)null, NetworkReaderExtensions.ReadString(reader));
		}
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<ChaosRewardType>(ref type, (Action<ChaosRewardType, ChaosRewardType>)null, GeneratedNetworkCode._Read_ChaosRewardType(reader));
		}
		if ((num & 0x400L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref amount, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
