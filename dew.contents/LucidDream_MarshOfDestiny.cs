using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class LucidDream_MarshOfDestiny : LucidDream
{
	[SaveVar(SaveVarFlags.Default)]
	private List<float> _killTimes = new List<float>();

	private int _lastInsertIndex;

	[CompilerGenerated]
	[SyncVar]
	private BonusStats syncedMonsterStatBonus__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private BonusStats syncedMonsterStatBonusThisZone__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float addedMirageSkinChance__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float addedMirageSkinChanceThisZone__BackingField;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncDictionary<SyncableAssetRef, float> addedMirageSkinChanceByType = new SyncDictionary<SyncableAssetRef, float>();

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncDictionary<SyncableAssetRef, float> addedMirageSkinChanceThisZoneByType = new SyncDictionary<SyncableAssetRef, float>();

	[CompilerGenerated]
	[SyncVar]
	private bool doRandomCurseEveryTravelThisZone__BackingField;

	[NonSerialized]
	[SaveVar(SaveVarFlags.Default)]
	public readonly SyncList<SyncableAssetRef> heroicBossEffects = new SyncList<SyncableAssetRef>();

	[SaveVar(SaveVarFlags.Default)]
	public StatBonus monsterStatBonus { get; set; } = new StatBonus();

	[SaveVar(SaveVarFlags.Default)]
	public StatBonus monsterStatBonusThisZone { get; set; } = new StatBonus();

	public BonusStats syncedMonsterStatBonus
	{
		[CompilerGenerated]
		get
		{
			return syncedMonsterStatBonus__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CsyncedMonsterStatBonus_003Ek__BackingField = value;
		}
	}

	public BonusStats syncedMonsterStatBonusThisZone
	{
		[CompilerGenerated]
		get
		{
			return syncedMonsterStatBonusThisZone__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CsyncedMonsterStatBonusThisZone_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public float addedMirageSkinChance
	{
		[CompilerGenerated]
		get
		{
			return addedMirageSkinChance__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CaddedMirageSkinChance_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public float addedMirageSkinChanceThisZone
	{
		[CompilerGenerated]
		get
		{
			return addedMirageSkinChanceThisZone__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CaddedMirageSkinChanceThisZone_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public bool doRandomCurseEveryTravelThisZone
	{
		[CompilerGenerated]
		get
		{
			return doRandomCurseEveryTravelThisZone__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CdoRandomCurseEveryTravelThisZone_003Ek__BackingField = value;
		}
	}

	public BonusStats Network_003CsyncedMonsterStatBonus_003Ek__BackingField
	{
		get
		{
			return syncedMonsterStatBonus__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<BonusStats>(value, ref syncedMonsterStatBonus__BackingField, 8uL, (Action<BonusStats, BonusStats>)null);
		}
	}

	public BonusStats Network_003CsyncedMonsterStatBonusThisZone_003Ek__BackingField
	{
		get
		{
			return syncedMonsterStatBonusThisZone__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<BonusStats>(value, ref syncedMonsterStatBonusThisZone__BackingField, 16uL, (Action<BonusStats, BonusStats>)null);
		}
	}

	public float Network_003CaddedMirageSkinChance_003Ek__BackingField
	{
		get
		{
			return addedMirageSkinChance__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref addedMirageSkinChance__BackingField, 32uL, (Action<float, float>)null);
		}
	}

	public float Network_003CaddedMirageSkinChanceThisZone_003Ek__BackingField
	{
		get
		{
			return addedMirageSkinChanceThisZone__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref addedMirageSkinChanceThisZone__BackingField, 64uL, (Action<float, float>)null);
		}
	}

	public bool Network_003CdoRandomCurseEveryTravelThisZone_003Ek__BackingField
	{
		get
		{
			return doRandomCurseEveryTravelThisZone__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref doRandomCurseEveryTravelThisZone__BackingField, 128uL, (Action<bool, bool>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			Network_003CsyncedMonsterStatBonus_003Ek__BackingField = (BonusStats)monsterStatBonus;
			Network_003CsyncedMonsterStatBonusThisZone_003Ek__BackingField = (BonusStats)monsterStatBonusThisZone;
			NetworkedManagerBase<ClientEventManager>.instance.OnDeath += new Action<EventInfoKill>(OnDeath);
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd += new Action<Entity>(ClientEventOnEntityAdd);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
			if ((UnityEngine.Object)(object)SingletonDewNetworkBehaviour<Room>.instance != null)
			{
				ClientEventOnRoomLoaded(default);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnDeath -= new Action<EventInfoKill>(OnDeath);
			}
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
			{
				NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd -= new Action<Entity>(ClientEventOnEntityAdd);
			}
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded -= new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
			}
		}
	}

	private void ClientEventOnZoneLoaded(EventInfoLoadZone obj)
	{
		if (obj.isTraveling)
		{
			monsterStatBonusThisZone = new StatBonus();
			Network_003CsyncedMonsterStatBonusThisZone_003Ek__BackingField = default;
			Network_003CaddedMirageSkinChanceThisZone_003Ek__BackingField = 0f;
			((SyncIDictionary<SyncableAssetRef, float>)(object)addedMirageSkinChanceThisZoneByType).Clear();
			Network_003CdoRandomCurseEveryTravelThisZone_003Ek__BackingField = false;
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		SingletonDewNetworkBehaviour<Room>.instance.monsters.onAfterSpawn += new Action<Entity>(OnAfterSpawn);
		if (obj.isTraveling && NetworkedManagerBase<ZoneManager>.instance.currentZone != null && NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex >= 0 && NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.Start && Dew.FindActorOfType<Shrine_MarshOfDestiny_SeedOfTorment>().IsNullOrInactive() && !NetworkedManagerBase<ZoneManager>.instance.currentZone.useSpecialGeneration)
		{
			SingletonDewNetworkBehaviour<Room>.instance.props.TryGetGoodNodePosition(out var vector);
			Dew.CreateActor<Shrine_MarshOfDestiny_SeedOfTorment>(vector, null, this);
		}
		Actor[] array = NetworkedManagerBase<ActorManager>.instance.allActors.ToArray();
		foreach (Actor actor in array)
		{
			if (actor is CurseStatusEffect && actor.persistentData.GetDataOrDefault("LucidDream_MarshOfDestiny", "isTransient", defaultValue: false))
			{
				actor.Destroy();
			}
		}
		if (doRandomCurseEveryTravelThisZone && obj.isTraveling)
		{
			NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
			{
				if (this.IsNullOrInactive())
				{
					return;
				}
				CurseStatusEffect[] list = DewResources.FindAllByType<CurseStatusEffect>(ResourceLoadSettings.Light).ToArray();
				foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
				{
					try
					{
						CurseStatusEffect curseStatusEffect = Dew.SelectRandomWeightedInList(list, (CurseStatusEffect effect) => (!effect.IsViable(NetworkedManagerBase<ActorManager>.instance.allHeroes[0])) ? 0f : effect.chanceWeight, null);
						allHero.CreateStatusEffect(DewResources.GetByType<CurseStatusEffect>(((object)curseStatusEffect).GetType(), default(ResourceLoadSettings)), allHero, new CastInfo(allHero), (CurseStatusEffect se) =>
						{
							HatredStrengthType[] array2 = ((HatredStrengthType[])Enum.GetValues(typeof(HatredStrengthType))).Where((HatredStrengthType strength) => se.availableStrengths.HasFlag(strength)).ToArray();
							se.currentStrength = array2[UnityEngine.Random.Range(0, array2.Length)];
							se.disableStartNotification = true;
							se.disableEndNotification = true;
						}).persistentData.SetData("LucidDream_MarshOfDestiny", "isTransient", value: true);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
			});
		}
		SingletonDewNetworkBehaviour<Room>.instance.monsters.addedMirageChance += addedMirageSkinChance + addedMirageSkinChanceThisZone;
	}

	private void ClientEventOnEntityAdd(Entity ent)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (ent is Monster)
		{
			StatBonus statBonus = monsterStatBonus.Clone();
			statBonus.Add(monsterStatBonusThisZone);
			ent.Status.AddStatBonus(statBonus);
		}
		if (!(ent is BossMonster))
		{
			return;
		}
		Enumerator<SyncableAssetRef> enumerator = heroicBossEffects.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ent.CreateStatusEffect((MiniBossEffect)(object)enumerator.Current.asset, ent, new CastInfo(ent));
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private void OnDeath(EventInfoKill obj)
	{
		if (obj.victim is Monster monster && !monster.IsAnyBoss() && monster.type != Monster.MonsterType.Lesser)
		{
			float num = Time.time - obj.victim.creationTime;
			if (_killTimes.Count < 200)
			{
				_killTimes.Add(num);
				return;
			}
			_killTimes[_lastInsertIndex] = num;
			_lastInsertIndex = (_lastInsertIndex + 1) % _killTimes.Count;
		}
	}

	public float GetCurrentKillTimePerMonster()
	{
		if (_killTimes.Count == 0)
		{
			return 60f;
		}
		int num = 0;
		float num2 = 0f;
		List<float> list = _killTimes.ToListNonAlloc(out var handle);
		list.Sort();
		float num3 = list[list.Count / 2];
		int num4 = 0;
		if (list.Count > 10)
		{
			num4 = Mathf.RoundToInt((float)list.Count * 0.2f);
		}
		for (int i = num4; i < list.Count - num4; i++)
		{
			num++;
			num2 += list[i];
		}
		handle.Return();
		return num2 / (float)num * 0.5f + num3 * 0.5f;
	}

	public override string GetCustomInGameTooltip()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		string text = "<color=white>" + DewLocalization.GetUIValue("LucidDream") + "</color> <color=" + Dew.GetHex(color) + ">" + DewLocalization.GetUIValue(((object)this).GetType().Name + "_Name") + "</color>\n" + DewLocalization.GetUIValue(((object)this).GetType().Name + "_Description");
		Shrine_MarshOfDestiny_SeedOfTorment shrine = DewResources.GetByType<Shrine_MarshOfDestiny_SeedOfTorment>(ResourceLoadSettings.Light);
		Handle(isTemp: false);
		Enumerator<SyncableAssetRef> enumerator = heroicBossEffects.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				AddText(Shrine_MarshOfDestiny_SeedOfTorment.PenaltyType.HeroicBossSpecialSkill, enumerator.Current.lightAsset.GetType().Name);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Handle(isTemp: true);
		if (type == LucidDreamType.Evil)
		{
			text = text + "\n\n<color=#ff87cd>" + string.Format(DewLocalization.GetUIValue("HeroicBossKillStardustBonusTemplate"), 3) + "</color>";
		}
		return text;
		void AddText(Shrine_MarshOfDestiny_SeedOfTorment.PenaltyType type, object parameter, bool isTemp = false)
		{
			string choicePenaltyDescriptionAdv = shrine.GetChoicePenaltyDescriptionAdv(new Shrine_MarshOfDestiny_SeedOfTorment.ChoiceItem
			{
				penalty = type,
				penaltyParameter = ((parameter as string) ?? DewPersistence.ToJson(parameter)),
				isTempPenalty = isTemp
			});
			text = text + "\n<color=#ffa3a3>- " + DewAdvText.ConvertAdvToText(choicePenaltyDescriptionAdv) + "</color>";
		}
		void Handle(bool isTemp)
		{
			BonusStats bonusStats = (isTemp ? syncedMonsterStatBonusThisZone : syncedMonsterStatBonus);
			if (bonusStats.armorFlat > 0f)
			{
				AddText(Shrine_MarshOfDestiny_SeedOfTorment.PenaltyType.ArmorFlat, bonusStats.armorFlat, isTemp);
			}
			if (bonusStats.maxHealthPercentage > 0f)
			{
				AddText(Shrine_MarshOfDestiny_SeedOfTorment.PenaltyType.MaxHealthPercentage, bonusStats.maxHealthPercentage, isTemp);
			}
			if (bonusStats.attackSpeedPercentage > 0f)
			{
				AddText(Shrine_MarshOfDestiny_SeedOfTorment.PenaltyType.AtkSpdPercentage, bonusStats.attackSpeedPercentage, isTemp);
			}
			if (bonusStats.movementSpeedPercentage > 0f)
			{
				AddText(Shrine_MarshOfDestiny_SeedOfTorment.PenaltyType.MovSpdPercentage, bonusStats.movementSpeedPercentage, isTemp);
			}
			float num = (isTemp ? addedMirageSkinChanceThisZone : addedMirageSkinChance);
			if (num > 0f)
			{
				AddText(Shrine_MarshOfDestiny_SeedOfTorment.PenaltyType.MirageSkinAnyPercentage, num * 100f, isTemp);
			}
			foreach (KeyValuePair<SyncableAssetRef, float> item in isTemp ? addedMirageSkinChanceThisZoneByType : addedMirageSkinChanceByType)
			{
				AddText(Shrine_MarshOfDestiny_SeedOfTorment.PenaltyType.MirageSkinSpecificPercentage, (item.Key.lightAsset.GetType().Name, item.Value * 100f), isTemp);
			}
		}
	}

	private void OnAfterSpawn(Entity ent)
	{
		Dew.CallDelayed(() =>
		{
			if (!ent.IsNullOrInactive() && ent is Monster monster && !ent.IsAnyBoss() && monster.type != Monster.MonsterType.Lesser && !monster.Status.HasStatusEffect<MirageSkinEffect>() && !TryDict((IDictionary<SyncableAssetRef, float>)addedMirageSkinChanceThisZoneByType))
			{
				TryDict((IDictionary<SyncableAssetRef, float>)addedMirageSkinChanceByType);
			}
		});
		bool TryDict(IDictionary<SyncableAssetRef, float> dict)
		{
			float value = UnityEngine.Random.value;
			float num = 0f;
			foreach (KeyValuePair<SyncableAssetRef, float> item in dict)
			{
				num += item.Value;
				if (num >= value)
				{
					ent.CreateStatusEffect((MirageSkinEffect)(object)item.Key.asset, ent, new CastInfo(ent));
					return true;
				}
			}
			return false;
		}
	}

	public LucidDream_MarshOfDestiny()
	{
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)addedMirageSkinChanceByType);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)addedMirageSkinChanceThisZoneByType);
		((NetworkBehaviour)this).InitSyncObject((SyncObject)(object)heroicBossEffects);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_BonusStats(writer, syncedMonsterStatBonus__BackingField);
			GeneratedNetworkCode._Write_BonusStats(writer, syncedMonsterStatBonusThisZone__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, addedMirageSkinChance__BackingField);
			NetworkWriterExtensions.WriteFloat(writer, addedMirageSkinChanceThisZone__BackingField);
			NetworkWriterExtensions.WriteBool(writer, doRandomCurseEveryTravelThisZone__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8L) != 0L)
		{
			GeneratedNetworkCode._Write_BonusStats(writer, syncedMonsterStatBonus__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10L) != 0L)
		{
			GeneratedNetworkCode._Write_BonusStats(writer, syncedMonsterStatBonusThisZone__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, addedMirageSkinChance__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, addedMirageSkinChanceThisZone__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, doRandomCurseEveryTravelThisZone__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<BonusStats>(ref syncedMonsterStatBonus__BackingField, (Action<BonusStats, BonusStats>)null, GeneratedNetworkCode._Read_BonusStats(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<BonusStats>(ref syncedMonsterStatBonusThisZone__BackingField, (Action<BonusStats, BonusStats>)null, GeneratedNetworkCode._Read_BonusStats(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref addedMirageSkinChance__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref addedMirageSkinChanceThisZone__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref doRandomCurseEveryTravelThisZone__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 8L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<BonusStats>(ref syncedMonsterStatBonus__BackingField, (Action<BonusStats, BonusStats>)null, GeneratedNetworkCode._Read_BonusStats(reader));
		}
		if ((num & 0x10L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<BonusStats>(ref syncedMonsterStatBonusThisZone__BackingField, (Action<BonusStats, BonusStats>)null, GeneratedNetworkCode._Read_BonusStats(reader));
		}
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref addedMirageSkinChance__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref addedMirageSkinChanceThisZone__BackingField, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref doRandomCurseEveryTravelThisZone__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
