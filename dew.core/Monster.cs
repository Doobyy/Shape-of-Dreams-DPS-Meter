using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Monster : Entity
{
	public enum MonsterType : byte
	{
		Lesser,
		Normal,
		MiniBoss,
		Boss
	}

	[Serializable]
	public class PoolAbilityInfo
	{
		public AssetRef<AbilityTrigger> triggerPrefab;

		[Range(0f, 1f)]
		public float chance = 1f;

		public bool healthConditionEnable;

		public Vector2 normalizedHealthRange = new Vector2(0f, 1f);

		public AbilityTrigger triggerInstance { get; set; }
	}

	[Serializable]
	public class MonsterAbilityPool
	{
		public List<PoolAbilityInfo> abilities = new List<PoolAbilityInfo>();

		public AssetRef<AttackTrigger> attackTriggerPrefab;
	}

	[SyncVar]
	public MonsterType type = MonsterType.Normal;

	public float chaseRandomness = 0.5f;

	public float chasePredictiveness = 0.5f;

	[CompilerGenerated]
	[SyncVar(hook = "OnPopulationChanged")]
	[SerializeField]
	private float populationCost__BackingField = 1f;

	[CompilerGenerated]
	[SyncVar]
	private bool isHunter__BackingField;

	[NonSerialized]
	public bool disableLoot;

	public List<MonsterAbilityPool> abilityPools = new List<MonsterAbilityPool>();

	public Action<float, float> _Mirror_SyncVarHookDelegate__003CpopulationCost_003Ek__BackingField;

	protected override DewPlayer defaultOwner => DewPlayer.creep;

	public virtual Vector3? spawnPosOverride => null;

	public virtual Quaternion? spawnRotOverride => null;

	public float populationCost
	{
		[CompilerGenerated]
		get
		{
			return populationCost__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CpopulationCost_003Ek__BackingField = value;
		}
	}

	public bool isHunter
	{
		[CompilerGenerated]
		get
		{
			return isHunter__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisHunter_003Ek__BackingField = value;
		}
	}

	[SaveVar(SaveVarFlags.Default)]
	public Vector3? campPosition { get; internal set; }

	public MonsterAbilityPool selectedPool => abilityPools.GetOrDefault(currentPoolIndex);

	public int currentPoolIndex { get; set; } = -1;

	public MonsterType Networktype
	{
		get
		{
			return type;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<MonsterType>(value, ref type, 32uL, (Action<MonsterType, MonsterType>)null);
		}
	}

	public float Network_003CpopulationCost_003Ek__BackingField
	{
		get
		{
			return populationCost__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<float>(value, ref populationCost__BackingField, 64uL, _Mirror_SyncVarHookDelegate__003CpopulationCost_003Ek__BackingField);
		}
	}

	public bool Network_003CisHunter_003Ek__BackingField
	{
		get
		{
			return isHunter__BackingField;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<bool>(value, ref isHunter__BackingField, 128uL, (Action<bool, bool>)null);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		float monsterBonusHealthPercentageByMultiplayer = NetworkedManagerBase<GameManager>.instance.GetMonsterBonusHealthPercentageByMultiplayer(this);
		float monsterBonusPowerPercentage = NetworkedManagerBase<GameManager>.instance.GetMonsterBonusPowerPercentage(this);
		Status.AddStatBonus(new StatBonus
		{
			maxHealthPercentage = monsterBonusHealthPercentageByMultiplayer,
			attackDamagePercentage = monsterBonusPowerPercentage,
			abilityPowerPercentage = monsterBonusPowerPercentage
		});
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if (!campPosition.HasValue)
			{
				NetworkedManagerBase<GameManager>.instance.MarkSpawnedPopulationDirty();
			}
			SelectAbilityPool();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance == null) && !campPosition.HasValue)
		{
			NetworkedManagerBase<GameManager>.instance.MarkSpawnedPopulationDirty();
		}
	}

	private void OnPopulationChanged(float oldValue, float newValue)
	{
		if (((NetworkBehaviour)this).isServer && isActive && !campPosition.HasValue)
		{
			NetworkedManagerBase<GameManager>.instance.MarkSpawnedPopulationDirty();
		}
	}

	protected override StaggerSettings GetStaggerSettings()
	{
		return type switch
		{
			MonsterType.Lesser => StaggerSettings.LesserMonsterDefault, 
			MonsterType.Normal => StaggerSettings.NormalMonsterDefault, 
			MonsterType.MiniBoss => StaggerSettings.MiniBossMonsterDefault, 
			MonsterType.Boss => StaggerSettings.BossMonsterDefault, 
			_ => new StaggerSettings
			{
				enabled = false
			}, 
		};
	}

	[Server]
	private void SelectAbilityPool()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Monster::SelectAbilityPool()' called when server was not active");
		}
		else if (abilityPools.Count > 0)
		{
			bool flag = (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null && NetworkedManagerBase<ZoneManager>.instance.isCurrentZoneHardVariant;
			currentPoolIndex = (flag ? Mathf.Min(1, abilityPools.Count - 1) : 0);
			if (this is BossMonster)
			{
				ZoneManager instance = NetworkedManagerBase<ZoneManager>.instance;
				Debug.Log($"[BossPool] {((UnityEngine.Object)(object)this).name}: pool {currentPoolIndex} of {abilityPools.Count} " + "(" + (flag ? "HARD VARIANT" : "normal") + "), zoneIndex=" + $"{(((UnityEngine.Object)(object)instance != null) ? instance.currentZoneIndex : (-1))}, hardZones=[" + (((UnityEngine.Object)(object)instance != null) ? string.Join(",", instance.hardVariantBossZoneIndices) : "") + "]");
			}
			AssignTriggerInstancesToCurrentPool();
		}
	}

	private void AssignTriggerInstancesToCurrentPool()
	{
		if ((UnityEngine.Object)(object)selectedPool.attackTriggerPrefab.asset != null)
		{
			AttackTrigger attackAbility = Dew.CreateActor(selectedPool.attackTriggerPrefab.asset, Vector3.zero, Quaternion.identity);
			Ability.SetAttackAbility(attackAbility);
		}
		AbilityTrigger[] array = (AbilityTrigger[])Ability.abilities.Values;
		foreach (PoolAbilityInfo ability in selectedPool.abilities)
		{
			foreach (AbilityTrigger abilityTrigger in array)
			{
				if (ability.triggerPrefab.type == ((object)abilityTrigger).GetType())
				{
					ability.triggerInstance = abilityTrigger;
					break;
				}
			}
		}
	}

	protected virtual bool PoolAIUpdate(ref EntityAIContext context, PoolAbilityInfo abilityInfo)
	{
		if ((UnityEngine.Object)(object)context.targetEnemy == null)
		{
			return false;
		}
		if (UnityEngine.Random.value > abilityInfo.chance * context.deltaTime)
		{
			return false;
		}
		if (!AI.Helper_CanBeCast(abilityInfo.triggerInstance))
		{
			return false;
		}
		if (abilityInfo.triggerInstance.configs[0].castMethod.type != CastMethodType.None && !AI.Helper_IsTargetInRange(abilityInfo.triggerInstance))
		{
			return false;
		}
		if (abilityInfo.healthConditionEnable && (normalizedHealth <= abilityInfo.normalizedHealthRange.x || normalizedHealth >= abilityInfo.normalizedHealthRange.y))
		{
			return false;
		}
		AI.Helper_CastAbilityAuto(abilityInfo.triggerInstance);
		return true;
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (selectedPool == null)
		{
			return;
		}
		for (int i = 0; i < selectedPool.abilities.Count; i++)
		{
			PoolAbilityInfo abilityInfo = selectedPool.abilities[i];
			if (PoolAIUpdate(ref context, abilityInfo))
			{
				break;
			}
		}
	}

	public Monster()
	{
		_Mirror_SyncVarHookDelegate__003CpopulationCost_003Ek__BackingField = OnPopulationChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_Monster_002FMonsterType(writer, type);
			NetworkWriterExtensions.WriteFloat(writer, populationCost__BackingField);
			NetworkWriterExtensions.WriteBool(writer, isHunter__BackingField);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x20L) != 0L)
		{
			GeneratedNetworkCode._Write_Monster_002FMonsterType(writer, type);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x40L) != 0L)
		{
			NetworkWriterExtensions.WriteFloat(writer, populationCost__BackingField);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x80L) != 0L)
		{
			NetworkWriterExtensions.WriteBool(writer, isHunter__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<MonsterType>(ref type, (Action<MonsterType, MonsterType>)null, GeneratedNetworkCode._Read_Monster_002FMonsterType(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref populationCost__BackingField, _Mirror_SyncVarHookDelegate__003CpopulationCost_003Ek__BackingField, NetworkReaderExtensions.ReadFloat(reader));
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isHunter__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x20L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<MonsterType>(ref type, (Action<MonsterType, MonsterType>)null, GeneratedNetworkCode._Read_Monster_002FMonsterType(reader));
		}
		if ((num & 0x40L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<float>(ref populationCost__BackingField, _Mirror_SyncVarHookDelegate__003CpopulationCost_003Ek__BackingField, NetworkReaderExtensions.ReadFloat(reader));
		}
		if ((num & 0x80L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<bool>(ref isHunter__BackingField, (Action<bool, bool>)null, NetworkReaderExtensions.ReadBool(reader));
		}
	}
}
