using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

[SaveActor(true)]
public class CurseStatusEffect : StatusEffect
{
	public SafeAction CurseEvent_OnProgressChanged;

	public HatredStrengthType availableStrengths = HatredStrengthType.Mild | HatredStrengthType.Potent | HatredStrengthType.Powerful;

	public float chanceWeight = 1f;

	[CompilerGenerated]
	[SyncVar]
	private HatredStrengthType currentStrength__BackingField = HatredStrengthType.Potent;

	[CompilerGenerated]
	[SyncVar]
	private int requiredAmount__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int currentProgress__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private QuestProgressType progressType__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private bool disableStartNotification__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private bool disableEndNotification__BackingField;

	private KillTracker _tracker;

	[NonSerialized]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public Quest_KillToLiftCurse quest;

	[NonSerialized]
	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public bool dontRemoveOnKnockOut;

	protected NetworkBehaviourSyncVar ___questNetId;

	[SaveVar(SaveVarFlags.Default)]
	public HatredStrengthType currentStrength
	{
		[CompilerGenerated]
		get
		{
			return currentStrength__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcurrentStrength_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int requiredAmount
	{
		[CompilerGenerated]
		get
		{
			return requiredAmount__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CrequiredAmount_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public int currentProgress
	{
		[CompilerGenerated]
		get
		{
			return currentProgress__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcurrentProgress_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public QuestProgressType progressType
	{
		[CompilerGenerated]
		get
		{
			return progressType__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CprogressType_003Ek__BackingField = value;
		}
	}

	public bool disableStartNotification
	{
		[CompilerGenerated]
		get
		{
			return disableStartNotification__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CdisableStartNotification_003Ek__BackingField = value;
		}
	}

	public bool disableEndNotification
	{
		[CompilerGenerated]
		get
		{
			return disableEndNotification__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CdisableEndNotification_003Ek__BackingField = value;
		}
	}

	public HatredStrengthType Network_003CcurrentStrength_003Ek__BackingField
	{
		get
		{
			return currentStrength__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<HatredStrengthType>(value, ref currentStrength__BackingField, 4096uL, (Action<HatredStrengthType, HatredStrengthType>)null);
		}
	}

	public int Network_003CrequiredAmount_003Ek__BackingField
	{
		get
		{
			return requiredAmount__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref requiredAmount__BackingField, 8192uL, (Action<int, int>)null);
		}
	}

	public int Network_003CcurrentProgress_003Ek__BackingField
	{
		get
		{
			return currentProgress__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentProgress__BackingField, 16384uL, (Action<int, int>)null);
		}
	}

	public QuestProgressType Network_003CprogressType_003Ek__BackingField
	{
		get
		{
			return progressType__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<QuestProgressType>(value, ref progressType__BackingField, 32768uL, (Action<QuestProgressType, QuestProgressType>)null);
		}
	}

	public bool Network_003CdisableStartNotification_003Ek__BackingField
	{
		get
		{
			return disableStartNotification__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref disableStartNotification__BackingField, 65536uL, (Action<bool, bool>)null);
		}
	}

	public bool Network_003CdisableEndNotification_003Ek__BackingField
	{
		get
		{
			return disableEndNotification__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref disableEndNotification__BackingField, 131072uL, (Action<bool, bool>)null);
		}
	}

	public Quest_KillToLiftCurse Networkquest
	{
		get
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return ((NetworkBehaviour)this).GetSyncVarNetworkBehaviour<Quest_KillToLiftCurse>(___questNetId, ref quest);
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter_NetworkBehaviour<Quest_KillToLiftCurse>(value, ref quest, 262144uL, (Action<Quest_KillToLiftCurse, Quest_KillToLiftCurse>)null, ref ___questNetId);
		}
	}

	public bool NetworkdontRemoveOnKnockOut
	{
		get
		{
			return dontRemoveOnKnockOut;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref dontRemoveOnKnockOut, 524288uL, (Action<bool, bool>)null);
		}
	}

	public override void ClearPooledEventsAndProcessors()
	{
		base.ClearPooledEventsAndProcessors();
		CurseEvent_OnProgressChanged?.Clear();
	}

	[LoadCallback(SaveVarFlags.Default)]
	private void OnLoad()
	{
		Network_003CdisableStartNotification_003Ek__BackingField = true;
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();
		if (skillLevel == -1)
		{
			skillLevel = Mathf.Max(0, (int)(currentStrength - 1));
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		string name = GetName();
		if (((NetworkBehaviour)victim).isOwned && !disableStartNotification)
		{
			InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_Curse_YouHaveBeenCursed");
		}
		if (!disableStartNotification)
		{
			string text = "#f57162";
			string text2 = string.Format(DewLocalization.GetUIValue("Chat_PlayerCursed"), "<color=white>" + ChatManager.GetDescribedPlayerName(victim.owner) + "</color>", "<color=white>" + name + "</color>");
			NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
			{
				type = ChatManager.MessageType.Raw,
				content = "<color=" + text + ">" + text2 + "</color>"
			});
		}
		icon = Resources.Load<Sprite>("Sprites/texCurse");
		iconOrder = 1;
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		showIcon = true;
		_tracker = victim.TrackKills(8f, OnKill);
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
		((Hero)victim).ClientHeroEvent_OnKnockedOut += new Action<EventInfoKill>(OnKnockOut);
		if (Networkquest.IsNullOrInactive())
		{
			Networkquest = NetworkedManagerBase<QuestManager>.instance.StartQuest((Quest_KillToLiftCurse q) =>
			{
				q.NetworktargetEffect = this;
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (!Networkquest.IsNullOrInactive())
		{
			if (!victim.IsNullInactiveDeadOrKnockedOut() && !disableEndNotification)
			{
				Networkquest.CompleteQuest();
			}
			else
			{
				Networkquest.Destroy();
			}
			Networkquest = null;
		}
		if (_tracker != null)
		{
			_tracker.Stop();
			_tracker = null;
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded -= new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
		}
		if (victim is Hero hero)
		{
			hero.ClientHeroEvent_OnKnockedOut -= new Action<EventInfoKill>(OnKnockOut);
		}
	}

	public virtual bool IsViable(Entity target)
	{
		return true;
	}

	private void OnKnockOut(EventInfoKill obj)
	{
		if (!dontRemoveOnKnockOut && isActive)
		{
			if (!Networkquest.IsNullOrInactive() && !disableEndNotification)
			{
				Networkquest.FailQuest(FailReason.NotSpecified);
			}
			Destroy();
		}
	}

	private void OnKill(EventInfoKill obj)
	{
		if (!isActive || !(obj.victim is Monster) || progressType != QuestProgressType.Kills)
		{
			return;
		}
		if (obj.victim is BossMonster)
		{
			for (int i = 0; i < 10; i++)
			{
				IncrementProgress();
			}
		}
		else
		{
			IncrementProgress();
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		if (!isActive || progressType != QuestProgressType.Travel || !obj.isTraveling)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			if (!SingletonDewNetworkBehaviour<Room>.instance.isRevisit)
			{
				IncrementProgress();
			}
		});
	}

	private void ClientEventOnZoneLoaded(EventInfoLoadZone obj)
	{
		if (isActive && progressType == QuestProgressType.TravelZone && obj.isTraveling)
		{
			NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(IncrementProgress);
		}
	}

	private void IncrementProgress()
	{
		if (!this.IsNullOrInactive())
		{
			currentProgress++;
			try
			{
				CurseEvent_OnProgressChanged?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			if (currentProgress >= requiredAmount)
			{
				Destroy();
			}
		}
	}

	public new T GetValue<T>(T[] arr)
	{
		return arr.GetClamped(currentStrength.GetValueIndex());
	}

	public string GetName()
	{
		string curseKey = DewLocalization.GetCurseKey(((object)this).GetType().Name);
		if (currentStrength == availableStrengths)
		{
			return DewLocalization.GetCurseName(curseKey);
		}
		return currentStrength switch
		{
			HatredStrengthType.None => DewLocalization.GetCurseName(curseKey), 
			HatredStrengthType.Mild => DewLocalization.GetCurseName(curseKey) + " I", 
			HatredStrengthType.Potent => DewLocalization.GetCurseName(curseKey) + " II", 
			HatredStrengthType.Powerful => DewLocalization.GetCurseName(curseKey) + " III", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_HatredStrengthType(writer, currentStrength__BackingField);
			NetworkWriterExtensions.WriteInt(writer, requiredAmount__BackingField);
			NetworkWriterExtensions.WriteInt(writer, currentProgress__BackingField);
			GeneratedNetworkCode._Write_QuestProgressType(writer, progressType__BackingField);
			NetworkWriterExtensions.WriteBool(writer, disableStartNotification__BackingField);
			NetworkWriterExtensions.WriteBool(writer, disableEndNotification__BackingField);
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Networkquest);
			NetworkWriterExtensions.WriteBool(writer, dontRemoveOnKnockOut);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			GeneratedNetworkCode._Write_HatredStrengthType(writer, currentStrength__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, requiredAmount__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x4000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentProgress__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000L) != 0L)
		{
			GeneratedNetworkCode._Write_QuestProgressType(writer, progressType__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, disableStartNotification__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, disableEndNotification__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteNetworkBehaviour(writer, (NetworkBehaviour)(object)Networkquest);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, dontRemoveOnKnockOut);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<HatredStrengthType>(ref currentStrength__BackingField, (Action<HatredStrengthType, HatredStrengthType>)null, GeneratedNetworkCode._Read_HatredStrengthType(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref requiredAmount__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentProgress__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<QuestProgressType>(ref progressType__BackingField, (Action<QuestProgressType, QuestProgressType>)null, GeneratedNetworkCode._Read_QuestProgressType(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref disableStartNotification__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref disableEndNotification__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Quest_KillToLiftCurse>(ref quest, (Action<Quest_KillToLiftCurse, Quest_KillToLiftCurse>)null, reader, ref ___questNetId);
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref dontRemoveOnKnockOut, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<HatredStrengthType>(ref currentStrength__BackingField, (Action<HatredStrengthType, HatredStrengthType>)null, GeneratedNetworkCode._Read_HatredStrengthType(reader));
		}
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref requiredAmount__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x4000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentProgress__BackingField, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x8000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<QuestProgressType>(ref progressType__BackingField, (Action<QuestProgressType, QuestProgressType>)null, GeneratedNetworkCode._Read_QuestProgressType(reader));
		}
		if ((num & 0x10000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref disableStartNotification__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x20000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref disableEndNotification__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize_NetworkBehaviour<Quest_KillToLiftCurse>(ref quest, (Action<Quest_KillToLiftCurse, Quest_KillToLiftCurse>)null, reader, ref ___questNetId);
		}
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref dontRemoveOnKnockOut, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
