using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ge_CallOfTheRavenous : GameEffect
{
	public const char LevelSeparator = ':';

	public int countForQuestActivate = 2;

	public float questActivationChance;

	public GameObject fxQuestActivate;

	[HideInInspector]
	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	public bool didSpawnQuest;

	[HideInInspector]
	[SaveVar(SaveVarFlags.Default)]
	[SyncVar(hook = "OnSkillCountChanged")]
	public int destroyedSkillCount;

	[HideInInspector]
	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	public bool isDestroyedIdentity;

	[HideInInspector]
	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	public bool enableSpawnScaredMaw;

	[SaveVar(SaveVarFlags.Default)]
	public Dictionary<string, List<string>> destroyedSkillData = new Dictionary<string, List<string>>();

	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	private bool _isQuestConditionMet;

	[SaveVar(SaveVarFlags.Default)]
	[SyncVar]
	private bool _didSpawnSidetrackRift;

	public Action<int, int> _Mirror_SyncVarHookDelegate_destroyedSkillCount;

	public bool NetworkdidSpawnQuest
	{
		get
		{
			return didSpawnQuest;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref didSpawnQuest, 8uL, (Action<bool, bool>)null);
		}
	}

	public int NetworkdestroyedSkillCount
	{
		get
		{
			return destroyedSkillCount;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref destroyedSkillCount, 16uL, _Mirror_SyncVarHookDelegate_destroyedSkillCount);
		}
	}

	public bool NetworkisDestroyedIdentity
	{
		get
		{
			return isDestroyedIdentity;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isDestroyedIdentity, 32uL, (Action<bool, bool>)null);
		}
	}

	public bool NetworkenableSpawnScaredMaw
	{
		get
		{
			return enableSpawnScaredMaw;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref enableSpawnScaredMaw, 64uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_isQuestConditionMet
	{
		get
		{
			return _isQuestConditionMet;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _isQuestConditionMet, 128uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_didSpawnSidetrackRift
	{
		get
		{
			return _didSpawnSidetrackRift;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref _didSpawnSidetrackRift, 256uL, (Action<bool, bool>)null);
		}
	}

	public void AddDestroyedSkill(string guid, SkillTrigger destroyedSkill, bool isIdentityDestroyed = false, int destroyedSkillLevel = -1)
	{
		if (!destroyedSkillData.TryGetValue(guid, out var value))
		{
			value = new List<string>();
			destroyedSkillData[guid] = value;
		}
		string name = ((object)destroyedSkill).GetType().Name;
		value.Add((destroyedSkillLevel >= 0) ? $"{name}{':'}{destroyedSkillLevel}" : name);
		if (isIdentityDestroyed)
		{
			NetworkisDestroyedIdentity = true;
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(OnRoomLoaded);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(OnRoomLoaded);
		}
	}

	private void OnSkillCountChanged(int oldVal, int newVal)
	{
		if (((NetworkBehaviour)this).isServer && newVal <= countForQuestActivate && (newVal >= countForQuestActivate || isDestroyedIdentity))
		{
			Network_isQuestConditionMet = true;
		}
	}

	private void OnRoomLoaded(EventInfoLoadRoom obj)
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (enableSpawnScaredMaw)
		{
			ChangeMawRoutine();
		}
		if (!didSpawnQuest && _isQuestConditionMet && obj.isTraveling && !obj.isSidetrackTransition && !(NetworkedManagerBase<ZoneManager>.instance.currentZone == null) && !NetworkedManagerBase<ZoneManager>.instance.currentZone.useSpecialGeneration && !NetworkedManagerBase<ZoneManager>.instance.nodes[obj.toIndex].HasMainModifier() && UnityEngine.Random.value < questActivationChance)
		{
			GameManager.CallOnReady(() =>
			{
				((MonoBehaviour)(object)this).StartCoroutine(Routine());
			});
		}
		IEnumerator Routine()
		{
			FxPlayNetworked(fxQuestActivate);
			yield return new WaitForSeconds(2f);
			NetworkedManagerBase<QuestManager>.instance.StartQuest<Quest_CallOfTheRavenous>();
			NetworkdidSpawnQuest = true;
		}
	}

	private void ChangeMawRoutine()
	{
		GameManager.CallOnReady(() =>
		{
			Shrine_MawOfDoom shrine_MawOfDoom = Dew.FindActorOfType<Shrine_MawOfDoom>();
			if (!((UnityEngine.Object)(object)shrine_MawOfDoom == null))
			{
				Vector3 vector = shrine_MawOfDoom.position;
				shrine_MawOfDoom.Destroy();
				Dew.CreateActor<Shrine_ScaredMaw>(vector, null);
			}
		});
	}

	public Ge_CallOfTheRavenous()
	{
		_Mirror_SyncVarHookDelegate_destroyedSkillCount = OnSkillCountChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteBool(writer, didSpawnQuest);
			NetworkWriterExtensions.WriteInt(writer, destroyedSkillCount);
			NetworkWriterExtensions.WriteBool(writer, isDestroyedIdentity);
			NetworkWriterExtensions.WriteBool(writer, enableSpawnScaredMaw);
			NetworkWriterExtensions.WriteBool(writer, _isQuestConditionMet);
			NetworkWriterExtensions.WriteBool(writer, _didSpawnSidetrackRift);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, didSpawnQuest);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, destroyedSkillCount);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isDestroyedIdentity);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, enableSpawnScaredMaw);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _isQuestConditionMet);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, _didSpawnSidetrackRift);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref didSpawnQuest, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref destroyedSkillCount, _Mirror_SyncVarHookDelegate_destroyedSkillCount, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isDestroyedIdentity, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref enableSpawnScaredMaw, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isQuestConditionMet, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _didSpawnSidetrackRift, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref didSpawnQuest, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref destroyedSkillCount, _Mirror_SyncVarHookDelegate_destroyedSkillCount, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isDestroyedIdentity, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref enableSpawnScaredMaw, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _isQuestConditionMet, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x100L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref _didSpawnSidetrackRift, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
