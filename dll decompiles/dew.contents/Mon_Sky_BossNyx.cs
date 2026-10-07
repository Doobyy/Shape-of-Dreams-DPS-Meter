using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Mon_Sky_BossNyx : BossMonster, IPrewarmMonsterContributor
{
	public int laserAtkSpawnCount = 6;

	public int lineAtkSpawnCount = 6;

	public float forceTeleportDistance;

	public float doubleTeleportChance;

	public float dashMinDistance = 11f;

	public GameObject[] phaseObjects;

	[SyncVar(hook = "OnCurrentPhaseChanged")]
	internal int _currentPhase;

	private int _currentPhaseBeforeChange;

	private float _starfallChance = 0.5f;

	public Action<int, int> _Mirror_SyncVarHookDelegate__currentPhase;

	public int Network_currentPhase
	{
		get
		{
			return _currentPhase;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).GeneratedSyncVarSetter<int>(value, ref _currentPhase, 512uL, _Mirror_SyncVarHookDelegate__currentPhase);
		}
	}

	public void ContributeMonsterPrewarm(Dictionary<Monster, int> counts, int instanceCount)
	{
		Ai_Mon_Sky_BossNyx_CreateSeeds byType = DewResources.GetByType<Ai_Mon_Sky_BossNyx_CreateSeeds>(default(ResourceLoadSettings));
		Mon_Sky_StarSeed byType2 = DewResources.GetByType<Mon_Sky_StarSeed>(default(ResourceLoadSettings));
		if (!((UnityEngine.Object)(object)byType == null) && !((UnityEngine.Object)(object)byType2 == null))
		{
			float num = (((UnityEngine.Object)(object)NetworkedManagerBase<GameManager>.instance != null) ? NetworkedManagerBase<GameManager>.instance.GetMultiplayerDifficultyFactor(reduceWhenDead: true) : 0f);
			int num2 = Mathf.RoundToInt(byType.spawnCount + byType.countPerExtraPlayer * num);
			counts.TryGetValue(byType2, out var value);
			counts[byType2] = value + num2 * 5 * instanceCount;
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepareSwipe(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Mon_Sky_BossNyx_Swipe ai_Mon_Sky_BossNyx_Swipe)
		{
			ai_Mon_Sky_BossNyx_Swipe.NetworkenableImprovedSwipe = true;
		}
	}

	private void OnCurrentPhaseChanged(int oldVal, int newVal)
	{
		if (!isActive)
		{
			return;
		}
		for (int i = 0; i < phaseObjects.Length; i++)
		{
			if (!(phaseObjects[i] == null))
			{
				phaseObjects[i].SetActive(newVal > i);
			}
		}
		if (newVal >= 2 && Ability.attackAbility is At_Mon_Sky_BossNyx_LaserAtk at_Mon_Sky_BossNyx_LaserAtk)
		{
			at_Mon_Sky_BossNyx_LaserAtk.spawnCount = laserAtkSpawnCount * 2;
		}
		if (newVal >= 2 && Ability.TryGetAbility<At_Mon_Sky_BossNyx_LineAtk>(out var trigger))
		{
			trigger.spawnCount = lineAtkSpawnCount * 2;
		}
		if (Ability.TryGetAbility<At_Mon_Sky_BossNyx_Swipe>(out var trigger2))
		{
			trigger2.configs[0].effectOnCast = ((newVal >= 1) ? trigger2.improvedCastEffect : trigger2.configs[0].effectOnCast);
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (Ability.TryGetAbility<At_Mon_Sky_BossNyx_Starfall>(out var trigger3))
		{
			trigger3.configs[0].maxCharges = ((newVal >= 1) ? 1 : 0);
			ResetCooldown(trigger3);
		}
		if (Ability.TryGetAbility<At_Mon_Sky_BossNyx_CreateSeeds>(out var trigger4))
		{
			trigger4.configs[0].maxCharges = ((newVal >= 1) ? 1 : 0);
			ResetCooldown(trigger4);
		}
		if (Ability.TryGetAbility<At_Mon_Sky_BossNyx_StarBuff>(out var trigger5))
		{
			trigger5.configs[0].maxCharges = ((newVal >= 1) ? 1 : 0);
			ResetCooldown(trigger5);
		}
		if (Ability.TryGetAbility<At_Mon_Sky_BossNyx_Teleport>(out var trigger6))
		{
			float cooldownTime = trigger6.configs[0].cooldownTime;
			trigger6.configs[0].cooldownTime = ((newVal < 1) ? cooldownTime : (cooldownTime -= cooldownTime / 3f));
			trigger6.configs[0].maxCharges = ((newVal < 1) ? 1 : 2);
			ResetCooldown(trigger6);
		}
		if (Ability.TryGetAbility<At_Mon_Sky_BossNyx_AltTeleport>(out var trigger7))
		{
			float cooldownTime2 = trigger7.configs[0].cooldownTime;
			trigger7.configs[0].cooldownTime = ((newVal < 1) ? cooldownTime2 : (cooldownTime2 -= cooldownTime2 / 3f));
			trigger7.configs[0].maxCharges = ((newVal < 1) ? 1 : 2);
			switch (newVal)
			{
			case 1:
				trigger7.maxTeleportCount = 2;
				break;
			case 2:
				trigger7.maxTeleportCount = 3;
				break;
			}
			ResetCooldown(trigger7);
		}
		if (Ability.TryGetAbility<At_Mon_Sky_BossNyx_Swipe>(out var trigger8))
		{
			if (newVal >= 1)
			{
				ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepareSwipe);
			}
			trigger8.configs[0].maxCharges = ((newVal < 1) ? 1 : 2);
			trigger8.configs[0].addedCharges = ((newVal < 2) ? 1 : 2);
			ResetCooldown(trigger8);
		}
		if (Ability.TryGetAbility<At_Mon_Sky_BossNyx_StellarDash>(out var trigger9))
		{
			trigger9.configs[0].maxCharges = ((newVal < 2) ? 1 : 3);
			trigger9.configs[0].addedCharges = ((newVal < 2) ? 1 : 3);
			ResetCooldown(trigger9);
		}
		if (Ability.TryGetAbility<At_Mon_Sky_BossNyx_Blackhole>(out var trigger10))
		{
			trigger10.configs[0].maxCharges = ((newVal >= 2) ? 1 : 0);
			ResetCooldown(trigger10);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		OnCurrentPhaseChanged(0, 0);
		if (((NetworkBehaviour)this).isServer)
		{
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
			EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			if (Ge_TheConsortOfNight.EnableSpawnErebos())
			{
				CreateStatusEffect<Se_Mon_Ink_BossDeathInterrupt>(this, new CastInfo(this));
			}
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (!Ge_TheConsortOfNight.EnableSpawnErebos())
		{
			if (_currentPhaseBeforeChange < 2 && normalizedHealth < 1f / 3f)
			{
				_starfallChance = 1f;
				_currentPhaseBeforeChange = 2;
				CreateStatusEffect<Se_Mon_Sky_BossNyx_PhaseChange>(this, new CastInfo(this));
			}
			else if (_currentPhaseBeforeChange < 1 && normalizedHealth < 2f / 3f)
			{
				_starfallChance = 0.7f;
				_currentPhaseBeforeChange = 1;
				CreateStatusEffect<Se_Mon_Sky_BossNyx_PhaseChange>(this, new CastInfo(this));
			}
		}
	}

	protected override bool PoolAIUpdate(ref EntityAIContext context, PoolAbilityInfo abilityInfo)
	{
		if ((UnityEngine.Object)(object)context.targetEnemy == null)
		{
			return false;
		}
		if (Status.HasStatusEffect<Se_Mon_Sky_BossNyx_PhaseChange>())
		{
			return true;
		}
		if (_currentPhaseBeforeChange == 2 && AI.Helper_CanBeCast<At_Mon_Sky_BossNyx_Blackhole>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Sky_BossNyx_Blackhole>();
			return true;
		}
		AbilityTrigger triggerInstance = abilityInfo.triggerInstance;
		if (!(triggerInstance is At_Mon_Sky_BossNyx_Starfall))
		{
			if (!(triggerInstance is At_Mon_Sky_BossNyx_Teleport trigger))
			{
				if (!(triggerInstance is At_Mon_Sky_BossNyx_AltTeleport trigger2))
				{
					if (triggerInstance is At_Mon_Sky_BossNyx_StellarDash)
					{
						if (AI.Helper_CanBeCast<At_Mon_Sky_BossNyx_StellarDash>() && AI.Helper_IsTargetInRange<At_Mon_Sky_BossNyx_StellarDash>() && AI.Helper_TryGetCastInfoAuto<At_Mon_Sky_BossNyx_StellarDash>(out var info))
						{
							if (Vector2.Distance(agentPosition.ToXY(), info.point.ToXY()) > dashMinDistance)
							{
								AI.Helper_CastAbility<At_Mon_Sky_BossNyx_StellarDash>(info);
								return true;
							}
							Vector3 validAgentDestination_Closest = Dew.GetValidAgentDestination_Closest(agentPosition, agentPosition + dashMinDistance * (info.point - agentPosition).normalized);
							if (Vector2.Distance(agentPosition.ToXY(), validAgentDestination_Closest.ToXY()) > dashMinDistance - 2f)
							{
								info.point = validAgentDestination_Closest;
								AI.Helper_CastAbility<At_Mon_Sky_BossNyx_StellarDash>(info);
								return true;
							}
						}
						return false;
					}
					return base.PoolAIUpdate(ref context, abilityInfo);
				}
				if (AI.Helper_CanBeCast<At_Mon_Sky_BossNyx_AltTeleport>())
				{
					AI.Helper_CastAbilityAuto<At_Mon_Sky_BossNyx_AltTeleport>();
					return true;
				}
				if (Vector3.Distance(context.targetEnemy.GetAIAgentPosition(this), agentPosition) >= forceTeleportDistance)
				{
					ResetCooldown(trigger2);
					AI.Helper_CastAbilityAuto<At_Mon_Sky_BossNyx_Teleport>();
					return true;
				}
				return false;
			}
			if (AI.Helper_CanBeCast<At_Mon_Sky_BossNyx_Teleport>())
			{
				if (!(UnityEngine.Random.value <= doubleTeleportChance * NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier()) || _currentPhase < 1)
				{
					AI.Helper_CastAbilityAuto<At_Mon_Sky_BossNyx_Teleport>();
					if (UnityEngine.Random.value > 0.7f)
					{
						return true;
					}
					if (Ability.TryGetAbility<At_Mon_Sky_BossNyx_Swipe>(out var trigger3))
					{
						ResetCooldown(trigger3);
					}
					return true;
				}
				AI.Helper_CastAbilityAuto<At_Mon_Sky_BossNyx_Teleport>();
				ResetCooldown(trigger);
				AI.Helper_CastAbilityAuto<At_Mon_Sky_BossNyx_Teleport>();
			}
			else if (Vector3.Distance(context.targetEnemy.GetAIAgentPosition(this), agentPosition) >= forceTeleportDistance)
			{
				ResetCooldown(trigger);
				AI.Helper_CastAbilityAuto<At_Mon_Sky_BossNyx_Teleport>();
				return true;
			}
			return false;
		}
		if (AI.Helper_CanBeCast<At_Mon_Sky_BossNyx_Starfall>() && UnityEngine.Random.value <= _starfallChance)
		{
			AI.Helper_CastAbilityAuto<At_Mon_Sky_BossNyx_Starfall>();
			return true;
		}
		return false;
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			if (_currentPhaseBeforeChange == 1 && AI.Helper_CanBeCast<At_Mon_Sky_BossNyx_StarBuff>())
			{
				Control.Attack(null, doChase: false);
			}
			else if (_currentPhaseBeforeChange == 2 && AI.Helper_CanBeCast<At_Mon_Sky_BossNyx_Blackhole>())
			{
				Control.Attack(null, doChase: false);
			}
			else
			{
				AI.Helper_ChaseTarget();
			}
		}
	}

	public override Type GetUniqueReward()
	{
		return typeof(St_U_HerWorld);
	}

	public Mon_Sky_BossNyx()
	{
		_Mirror_SyncVarHookDelegate__currentPhase = OnCurrentPhaseChanged;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteInt(writer, _currentPhase);
			return;
		}
		NetworkWriterExtensions.WriteULong(writer, ((NetworkBehaviour)this).syncVarDirtyBits);
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 0x200L) != 0L)
		{
			NetworkWriterExtensions.WriteInt(writer, _currentPhase);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _currentPhase, _Mirror_SyncVarHookDelegate__currentPhase, NetworkReaderExtensions.ReadInt(reader));
			return;
		}
		long num = (long)NetworkReaderExtensions.ReadULong(reader);
		if ((num & 0x200L) != 0L)
		{
			((NetworkBehaviour)this).GeneratedSyncVarDeserialize<int>(ref _currentPhase, _Mirror_SyncVarHookDelegate__currentPhase, NetworkReaderExtensions.ReadInt(reader));
		}
	}
}
