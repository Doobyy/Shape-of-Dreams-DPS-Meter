using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

[SaveActor(true)]
public class Se_Shrine_Chaos_StatBonus : PersistentStatBonusEffect
{
	[SyncVar]
	public StatBonus syncedRegularChaosBonus = new StatBonus();

	public float attackRangeBonus = 0.5f;

	public float critChanceBonus = 0.2f;

	public float fireAmpBonus = 0.3f;

	public float coldAmpBonus = 0.25f;

	public int addedMaxLightStack = 2;

	public int addedMaxDarkStack = 2;

	public int maxLevelBonus = 3;

	public int immediateCurrentLevelBonus = 1;

	public float speedPercentageBonus = 5f;

	public float abilHasteBonus = 5f;

	public float dodgeHaste = 20f;

	public int dodgeCharge = 1;

	public int startIndexBonus = 1;

	public int addedGemSlot = 1;

	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public float currentDodgeHaste;

	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public int currentDodgeAddedCharges;

	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public int currentAddedDarkMaxStack;

	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public int currentAddedLightMaxStack;

	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public int currentAddedMaxLevel;

	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public int currentAddedGemSlotQ;

	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public int currentAddedGemSlotW;

	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public int currentAddedGemSlotE;

	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public int currentAddedGemSlotR;

	[SyncVar]
	[SaveVar(SaveVarFlags.Default)]
	public int currentAddedGemSlotIdentity;

	[SaveVar(SaveVarFlags.Default)]
	public StatBonus corruptedBonus;

	[SyncVar]
	public StatBonus syncedCorruptedChaosBonus = new StatBonus();

	private ActorRef<Se_CorruptedChaos_BasicContainer> _basicContainer;

	public StatBonus NetworksyncedRegularChaosBonus
	{
		get
		{
			return syncedRegularChaosBonus;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<StatBonus>(value, ref syncedRegularChaosBonus, 4096uL, (Action<StatBonus, StatBonus>)null);
		}
	}

	public float NetworkcurrentDodgeHaste
	{
		get
		{
			return currentDodgeHaste;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref currentDodgeHaste, 8192uL, (Action<float, float>)null);
		}
	}

	public int NetworkcurrentDodgeAddedCharges
	{
		get
		{
			return currentDodgeAddedCharges;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentDodgeAddedCharges, 16384uL, (Action<int, int>)null);
		}
	}

	public int NetworkcurrentAddedDarkMaxStack
	{
		get
		{
			return currentAddedDarkMaxStack;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentAddedDarkMaxStack, 32768uL, (Action<int, int>)null);
		}
	}

	public int NetworkcurrentAddedLightMaxStack
	{
		get
		{
			return currentAddedLightMaxStack;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentAddedLightMaxStack, 65536uL, (Action<int, int>)null);
		}
	}

	public int NetworkcurrentAddedMaxLevel
	{
		get
		{
			return currentAddedMaxLevel;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentAddedMaxLevel, 131072uL, (Action<int, int>)null);
		}
	}

	public int NetworkcurrentAddedGemSlotQ
	{
		get
		{
			return currentAddedGemSlotQ;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentAddedGemSlotQ, 262144uL, (Action<int, int>)null);
		}
	}

	public int NetworkcurrentAddedGemSlotW
	{
		get
		{
			return currentAddedGemSlotW;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentAddedGemSlotW, 524288uL, (Action<int, int>)null);
		}
	}

	public int NetworkcurrentAddedGemSlotE
	{
		get
		{
			return currentAddedGemSlotE;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentAddedGemSlotE, 1048576uL, (Action<int, int>)null);
		}
	}

	public int NetworkcurrentAddedGemSlotR
	{
		get
		{
			return currentAddedGemSlotR;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentAddedGemSlotR, 2097152uL, (Action<int, int>)null);
		}
	}

	public int NetworkcurrentAddedGemSlotIdentity
	{
		get
		{
			return currentAddedGemSlotIdentity;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref currentAddedGemSlotIdentity, 4194304uL, (Action<int, int>)null);
		}
	}

	public StatBonus NetworksyncedCorruptedChaosBonus
	{
		get
		{
			return syncedCorruptedChaosBonus;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<StatBonus>(value, ref syncedCorruptedChaosBonus, 8388608uL, (Action<StatBonus, StatBonus>)null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NotifyUpdate();
		}
		OnCreate_Corrupted();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		OnDestroyActor_Corrupted();
	}

	[Server]
	public void NotifyUpdate()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_Shrine_Chaos_StatBonus::NotifyUpdate()' called when server was not active");
			return;
		}
		NetworksyncedRegularChaosBonus = bonus.Clone();
		NotifyUpdate_Corrupted();
	}

	public override string GetCustomTooltipName()
	{
		return DewLocalization.GetUIValue("Shrine_Chaos_Name");
	}

	public override string GetCustomTooltipDescription()
	{
		string text = "";
		if (!Mathf.Approximately(syncedRegularChaosBonus.maxHealthFlat, 0f))
		{
			text += string.Format("<color=yellow>{0}</color> +{1:#,##0}\n", DewLocalization.GetUIValue("Stat_Health"), syncedRegularChaosBonus.maxHealthFlat);
		}
		if (!Mathf.Approximately(syncedRegularChaosBonus.armorFlat, 0f))
		{
			text += string.Format("<color=yellow>{0}</color> +{1:#,##0}\n", DewLocalization.GetUIValue("Stat_Armor"), syncedRegularChaosBonus.armorFlat);
		}
		if (!Mathf.Approximately(syncedRegularChaosBonus.attackDamageFlat, 0f))
		{
			text += string.Format("<color=yellow>{0}</color> +{1:#,##0}\n", DewLocalization.GetUIValue("Stat_AttackDamage"), syncedRegularChaosBonus.attackDamageFlat);
		}
		if (!Mathf.Approximately(syncedRegularChaosBonus.abilityPowerFlat, 0f))
		{
			text += string.Format("<color=yellow>{0}</color> +{1:#,##0}\n", DewLocalization.GetUIValue("Stat_AbilityPower"), syncedRegularChaosBonus.abilityPowerFlat);
		}
		if (!Mathf.Approximately(syncedRegularChaosBonus.attackSpeedPercentage, 0f))
		{
			text += string.Format("<color=yellow>{0}</color> +{1:P0}\n", DewLocalization.GetUIValue("Stat_AttackSpeed"), syncedRegularChaosBonus.attackSpeedPercentage / 100f);
		}
		if (!Mathf.Approximately(syncedRegularChaosBonus.abilityHasteFlat, 0f))
		{
			text += string.Format("<color=yellow>{0}</color> +{1:#,##0}\n", DewLocalization.GetUIValue("Stat_SkillHaste"), syncedRegularChaosBonus.abilityHasteFlat);
		}
		text += GetCorruptedTooltipDescription();
		return text.Trim();
	}

	private void OnCreate_Corrupted()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			corruptedBonus = DoStatBonus(corruptedBonus);
			victim.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
		}
	}

	private void OnDestroyActor_Corrupted()
	{
		if (((NetworkBehaviour)this).isServer)
		{
			if (!_basicContainer.IsNullOrInactive())
			{
				_basicContainer.Get().Destroy();
			}
			_basicContainer = null;
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ActorEventOnDealDamage);
			}
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if (obj.damage.elemental == ElementalType.Light && obj.victim.Status.TryGetStatusEffect<Se_Elm_Light>(out var effect))
		{
			effect.maxStack = Mathf.Max(effect.maxStack, 5 + currentAddedLightMaxStack);
		}
		if (obj.damage.elemental == ElementalType.Dark && obj.victim.Status.TryGetStatusEffect<Se_Elm_Dark>(out var effect2))
		{
			effect2.maxStack = Mathf.Max(effect2.maxStack, 5 + currentAddedDarkMaxStack);
		}
	}

	[Server]
	public void AddCorruptedBonus(CorruptedChaosRewardType type)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_Shrine_Chaos_StatBonus::AddCorruptedBonus(CorruptedChaosRewardType)' called when server was not active");
			return;
		}
		switch (type)
		{
		case CorruptedChaosRewardType.AddedEssenceSlot:
			throw new InvalidOperationException();
		case CorruptedChaosRewardType.EveryFourAttackBonus:
			corruptedBonus.everyFourAttackStartIndexFlat += startIndexBonus;
			break;
		case CorruptedChaosRewardType.DodgeCooldown:
			NetworkcurrentDodgeHaste = currentDodgeHaste + dodgeHaste;
			break;
		case CorruptedChaosRewardType.DodgeCharge:
			NetworkcurrentDodgeAddedCharges = currentDodgeAddedCharges + dodgeCharge;
			break;
		case CorruptedChaosRewardType.AttackRange:
			corruptedBonus.attackRangeFlat += attackRangeBonus;
			break;
		case CorruptedChaosRewardType.CritChance:
			corruptedBonus.critChanceFlat += critChanceBonus;
			break;
		case CorruptedChaosRewardType.FireAmp:
			corruptedBonus.fireEffectAmpFlat += fireAmpBonus;
			break;
		case CorruptedChaosRewardType.DarkMaxStack:
			NetworkcurrentAddedDarkMaxStack = currentAddedDarkMaxStack + addedMaxDarkStack;
			break;
		case CorruptedChaosRewardType.LightMaxStack:
			NetworkcurrentAddedLightMaxStack = currentAddedLightMaxStack + addedMaxLightStack;
			break;
		case CorruptedChaosRewardType.ColdAmp:
			corruptedBonus.coldEffectAmpFlat += coldAmpBonus;
			break;
		case CorruptedChaosRewardType.MaxLevel:
			NetworkcurrentAddedMaxLevel = currentAddedMaxLevel + maxLevelBonus;
			Dew.CallDelayed(() =>
			{
				if (victim is Hero { Status: var status })
				{
					status.level += immediateCurrentLevelBonus;
				}
			});
			break;
		case CorruptedChaosRewardType.MovSpdSkillHaste:
			corruptedBonus.movementSpeedPercentage += speedPercentageBonus;
			corruptedBonus.abilityHasteFlat += abilHasteBonus;
			break;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
		NotifyUpdate();
	}

	[Server]
	public void AddEssenceSlotBonus(HeroSkillLocation location)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_Shrine_Chaos_StatBonus::AddEssenceSlotBonus(HeroSkillLocation)' called when server was not active");
			return;
		}
		switch (location)
		{
		case HeroSkillLocation.Q:
			NetworkcurrentAddedGemSlotQ = currentAddedGemSlotQ + addedGemSlot;
			break;
		case HeroSkillLocation.W:
			NetworkcurrentAddedGemSlotW = currentAddedGemSlotW + addedGemSlot;
			break;
		case HeroSkillLocation.E:
			NetworkcurrentAddedGemSlotE = currentAddedGemSlotE + addedGemSlot;
			break;
		case HeroSkillLocation.R:
			NetworkcurrentAddedGemSlotR = currentAddedGemSlotR + addedGemSlot;
			break;
		case HeroSkillLocation.Identity:
			NetworkcurrentAddedGemSlotIdentity = currentAddedGemSlotIdentity + addedGemSlot;
			break;
		default:
			throw new ArgumentOutOfRangeException("location", location, null);
		}
		NotifyUpdate();
	}

	private void NotifyUpdate_Corrupted()
	{
		Hero obj = (victim as Hero) ?? throw new InvalidOperationException();
		NetworksyncedCorruptedChaosBonus = corruptedBonus.Clone();
		if (!_basicContainer.IsNullOrInactive())
		{
			_basicContainer.Get().Destroy();
		}
		_basicContainer = CreateStatusEffect<Se_CorruptedChaos_BasicContainer>(victim);
		_basicContainer.Get().DoAbility((AbilityTrigger t) => ((object)t).GetType().Name.Contains("_M_"), (AbilityTrigger trg) =>
		{
			if (!(trg is SkillTrigger skillTrigger))
			{
				return (Action)null;
			}
			SkillBonus bonus = skillTrigger.AddSkillBonus(new SkillBonus
			{
				addedCharge = currentDodgeAddedCharges,
				cooldownMultiplier = 1f / (1f + currentDodgeHaste * 0.01f),
				ignoreReceiveCooldownReductionFlag = true
			});
			return () =>
			{
				bonus.Stop();
			};
		});
		obj.maxLevelOverride = NetworkedManagerBase<GameManager>.instance.ges.maxHeroLevel + currentAddedMaxLevel;
		obj.Skill.SetMaxGemCount(HeroSkillLocation.Q, 3 + currentAddedGemSlotQ);
		obj.Skill.SetMaxGemCount(HeroSkillLocation.W, 3 + currentAddedGemSlotW);
		obj.Skill.SetMaxGemCount(HeroSkillLocation.E, 3 + currentAddedGemSlotE);
		obj.Skill.SetMaxGemCount(HeroSkillLocation.R, 3 + currentAddedGemSlotR);
		obj.Skill.SetMaxGemCount(HeroSkillLocation.Identity, currentAddedGemSlotIdentity);
	}

	private string GetCorruptedTooltipDescription()
	{
		string text = "";
		string arg = "#f27663";
		int num = currentAddedGemSlotQ + currentAddedGemSlotW + currentAddedGemSlotE + currentAddedGemSlotR + currentAddedGemSlotIdentity;
		if (num != 0)
		{
			text += string.Format("<color={0}>{1}</color> +{2:#,##0}\n", arg, DewLocalization.GetUIValue("Shrine_CorruptedChaos_AddedEssenceSlot_Stat"), num);
		}
		if (syncedCorruptedChaosBonus.everyFourAttackStartIndexFlat != 0)
		{
			text += string.Format("<color={0}>{1}</color> -{2:#,##0}\n", arg, DewLocalization.GetUIValue("Shrine_CorruptedChaos_EveryFourAttackBonus_Stat"), syncedCorruptedChaosBonus.everyFourAttackStartIndexFlat);
		}
		if (!Mathf.Approximately(currentDodgeHaste, 0f))
		{
			text += string.Format("<color={0}>{1}</color> +{2:#,##0}\n", arg, DewLocalization.GetUIValue("Shrine_CorruptedChaos_DodgeCooldown_Stat"), currentDodgeHaste);
		}
		if (currentDodgeAddedCharges != 0)
		{
			text += string.Format("<color={0}>{1}</color> +{2:#,##0}\n", arg, DewLocalization.GetUIValue("Shrine_CorruptedChaos_DodgeCharge_Stat"), currentDodgeAddedCharges);
		}
		if (!Mathf.Approximately(syncedCorruptedChaosBonus.attackRangeFlat, 0f))
		{
			text += string.Format("<color={0}>{1}</color> +{2:#,##0.#}m\n", arg, DewLocalization.GetUIValue("Shrine_CorruptedChaos_AttackRange_Stat"), syncedCorruptedChaosBonus.attackRangeFlat);
		}
		if (!Mathf.Approximately(syncedCorruptedChaosBonus.critChanceFlat, 0f))
		{
			text += string.Format("<color={0}>{1}</color> +{2:P0}\n", arg, DewLocalization.GetUIValue("Stat_CritChance"), syncedCorruptedChaosBonus.critChanceFlat);
		}
		if (!Mathf.Approximately(syncedCorruptedChaosBonus.fireEffectAmpFlat, 0f))
		{
			text += string.Format("<color={0}>{1}</color> +{2:P0}\n", arg, DewLocalization.GetUIValue("Stat_FireAmp"), syncedCorruptedChaosBonus.fireEffectAmpFlat);
		}
		if (currentAddedDarkMaxStack != 0)
		{
			text += string.Format("<color={0}>{1}</color> +{2:#,##0}\n", arg, DewLocalization.GetUIValue("Shrine_CorruptedChaos_DarkMaxStack_Stat"), currentAddedDarkMaxStack);
		}
		if (currentAddedLightMaxStack != 0)
		{
			text += string.Format("<color={0}>{1}</color> +{2:#,##0}\n", arg, DewLocalization.GetUIValue("Shrine_CorruptedChaos_LightMaxStack_Stat"), currentAddedLightMaxStack);
		}
		if (!Mathf.Approximately(syncedCorruptedChaosBonus.coldEffectAmpFlat, 0f))
		{
			text += string.Format("<color={0}>{1}</color> +{2:P0}\n", arg, DewLocalization.GetUIValue("Shrine_CorruptedChaos_ColdAmp_Stat"), syncedCorruptedChaosBonus.coldEffectAmpFlat);
		}
		if (currentAddedMaxLevel != 0)
		{
			text += string.Format("<color={0}>{1}</color> +{2:#,##0}\n", arg, DewLocalization.GetUIValue("Shrine_CorruptedChaos_MaxLevel_Stat"), currentAddedMaxLevel);
		}
		if (!Mathf.Approximately(syncedCorruptedChaosBonus.movementSpeedPercentage, 0f))
		{
			text += string.Format("<color={0}>{1}</color> +{2:P0}\n", arg, DewLocalization.GetUIValue("Stat_MovementSpeed"), syncedCorruptedChaosBonus.movementSpeedPercentage * 0.01f);
		}
		if (!Mathf.Approximately(syncedCorruptedChaosBonus.abilityHasteFlat, 0f))
		{
			text += string.Format("<color={0}>{1}</color> +{2:#,##0}\n", arg, DewLocalization.GetUIValue("Stat_SkillHaste"), syncedCorruptedChaosBonus.abilityHasteFlat);
		}
		return text.Trim();
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_StatBonus(writer, syncedRegularChaosBonus);
			NetworkWriterExtensions.WriteFloat(writer, currentDodgeHaste);
			NetworkWriterExtensions.WriteInt(writer, currentDodgeAddedCharges);
			NetworkWriterExtensions.WriteInt(writer, currentAddedDarkMaxStack);
			NetworkWriterExtensions.WriteInt(writer, currentAddedLightMaxStack);
			NetworkWriterExtensions.WriteInt(writer, currentAddedMaxLevel);
			NetworkWriterExtensions.WriteInt(writer, currentAddedGemSlotQ);
			NetworkWriterExtensions.WriteInt(writer, currentAddedGemSlotW);
			NetworkWriterExtensions.WriteInt(writer, currentAddedGemSlotE);
			NetworkWriterExtensions.WriteInt(writer, currentAddedGemSlotR);
			NetworkWriterExtensions.WriteInt(writer, currentAddedGemSlotIdentity);
			GeneratedNetworkCode._Write_StatBonus(writer, syncedCorruptedChaosBonus);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x1000L) != 0L)
		{
			GeneratedNetworkCode._Write_StatBonus(writer, syncedRegularChaosBonus);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x2000L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, currentDodgeHaste);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x4000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentDodgeAddedCharges);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x8000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentAddedDarkMaxStack);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x10000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentAddedLightMaxStack);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentAddedMaxLevel);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentAddedGemSlotQ);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentAddedGemSlotW);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x100000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentAddedGemSlotE);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentAddedGemSlotR);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x400000L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, currentAddedGemSlotIdentity);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x800000L) != 0L)
		{
			GeneratedNetworkCode._Write_StatBonus(writer, syncedCorruptedChaosBonus);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<StatBonus>(ref syncedRegularChaosBonus, (Action<StatBonus, StatBonus>)null, GeneratedNetworkCode._Read_StatBonus(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref currentDodgeHaste, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentDodgeAddedCharges, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentAddedDarkMaxStack, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentAddedLightMaxStack, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentAddedMaxLevel, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentAddedGemSlotQ, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentAddedGemSlotW, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentAddedGemSlotE, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentAddedGemSlotR, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentAddedGemSlotIdentity, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<StatBonus>(ref syncedCorruptedChaosBonus, (Action<StatBonus, StatBonus>)null, GeneratedNetworkCode._Read_StatBonus(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x1000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<StatBonus>(ref syncedRegularChaosBonus, (Action<StatBonus, StatBonus>)null, GeneratedNetworkCode._Read_StatBonus(reader));
		}
		if ((num & 0x2000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref currentDodgeHaste, (Action<float, float>)null, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x4000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentDodgeAddedCharges, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x8000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentAddedDarkMaxStack, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x10000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentAddedLightMaxStack, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x20000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentAddedMaxLevel, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x40000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentAddedGemSlotQ, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x80000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentAddedGemSlotW, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x100000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentAddedGemSlotE, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x200000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentAddedGemSlotR, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x400000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref currentAddedGemSlotIdentity, (Action<int, int>)null, NetworkReaderExtensions.ReadInt(reader));
		}
		if ((num & 0x800000L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<StatBonus>(ref syncedCorruptedChaosBonus, (Action<StatBonus, StatBonus>)null, GeneratedNetworkCode._Read_StatBonus(reader));
		}
	}
}
