using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Gem_L_CamillasGift : Gem, IIgnoreFirstGemUpgrade
{
	public GameObject fxBreak;

	public float healthThreshold = 0.5f;

	[NonSerialized]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public int sellGoldIncreasePerWorld;

	public int currentSellGold => sellGoldOverride.GetValueOrDefault();

	public int NetworksellGoldIncreasePerWorld
	{
		get
		{
			return sellGoldIncreasePerWorld;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref sellGoldIncreasePerWorld, 262144uL, (Action<int, int>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded -= new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
		}
	}

	private void ClientEventOnZoneLoaded(EventInfoLoadZone obj)
	{
		if (obj.isTraveling && NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex != 0)
		{
			sellGoldOverride = sellGoldOverride.GetValueOrDefault() + sellGoldIncreasePerWorld;
		}
	}

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldOwner != null)
		{
			oldOwner.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		Entity entity = obj.actor.firstEntity;
		if (!((UnityEngine.Object)(object)entity == null) && owner.CheckEnemyOrNeutral(entity) && !(owner.normalizedHealth > healthThreshold))
		{
			FxPlayNewNetworked(fxBreak, owner);
			Hero hero = owner;
			owner.Skill.TryGetGemLocation(this, out var loc);
			owner.Skill.UnequipGem(loc, Vector3.zero);
			Destroy();
			Gem_C_CamillasGiftRuined gem = Dew.CreateGem<Gem_C_CamillasGiftRuined>(hero.position, 10, hero.owner);
			hero.Skill.EquipGem(loc, gem);
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
			NetworkWriterExtensions.WriteInt(writer, sellGoldIncreasePerWorld);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, sellGoldIncreasePerWorld);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref sellGoldIncreasePerWorld, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref sellGoldIncreasePerWorld, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
