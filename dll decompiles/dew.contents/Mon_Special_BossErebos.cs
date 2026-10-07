using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Mon_Special_BossErebos : BossMonster, IPrewarmMonsterContributor
{
	public float phaseChangeHealthThreshold = 0.5f;

	public GameObject fxPhaseChanged;

	public StatBonus phaseChangeStatBonus;

	[Space(15f)]
	public float starRainChance;

	public float teleportChance;

	public float spawnTormentorChance;

	public float spawnMeteorChance;

	public float rippleChance;

	public float gazeChance;

	[Space(10f)]
	public float spawnBlackholeChance;

	public float antiGravityChance;

	[Space(15f)]
	public GameObject[] vfxObjects;

	internal bool _isPhaseChanged;

	public void ContributeMonsterPrewarm(Dictionary<Monster, int> counts, int instanceCount)
	{
		Se_Mon_Special_BossErebos_PhaseChange byType = DewResources.GetByType<Se_Mon_Special_BossErebos_PhaseChange>(default(ResourceLoadSettings));
		Mon_Sky_StarSeed byType2 = DewResources.GetByType<Mon_Sky_StarSeed>(default(ResourceLoadSettings));
		if (!((UnityEngine.Object)(object)byType == null) && !((UnityEngine.Object)(object)byType2 == null))
		{
			counts.TryGetValue(byType2, out var value);
			counts[byType2] = value + byType.spawnSeedCount * instanceCount;
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		Visual.ClientEvent_OnRendererEnabledChanged += new Action<bool>(OnRendererEnableChanged);
		if (((NetworkBehaviour)this).isServer)
		{
			_isPhaseChanged = false;
			CreateStatusEffect<Se_Mon_Ink_BossDeathInterrupt>(this, new CastInfo(this));
			CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
			EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	private void OnRendererEnableChanged(bool obj)
	{
		if (obj)
		{
			GameObject[] array = vfxObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
		}
		else
		{
			GameObject[] array = vfxObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (!Status.HasStatusEffect<Se_Mon_Special_BossErebos_PhaseChange>() && normalizedHealth < phaseChangeHealthThreshold && !_isPhaseChanged)
		{
			_isPhaseChanged = true;
			CreateStatusEffect<Se_Mon_Special_BossErebos_PhaseChange>(this, new CastInfo(this));
			Dew.CallDelayed(() =>
			{
				Status.SetHealth(maxHealth * phaseChangeHealthThreshold);
			});
			if (Ability.TryGetAbility<At_Mon_Special_BossErebos_Ripple>(out var trigger))
			{
				trigger.configs[0].maxCharges = 3;
				trigger.configs[0].addedCharges = 3;
			}
			Status.AddStatBonus(phaseChangeStatBonus);
			FxPlayNetworked(fxPhaseChanged, this);
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((UnityEngine.Object)(object)context.targetEnemy == null))
		{
			if (UnityEngine.Random.value < gazeChance * context.deltaTime && normalizedHealth < 0.75f && AI.Helper_CanBeCast<At_Mon_Special_BossErebos_Gaze>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossErebos_Gaze>();
			}
			if (_isPhaseChanged && UnityEngine.Random.value < spawnBlackholeChance * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Special_BossErebos_SpawnBlackhole>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossErebos_SpawnBlackhole>();
			}
			else if (UnityEngine.Random.value < teleportChance * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Special_BossErebos_Teleport>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossErebos_Teleport>();
			}
			else if (UnityEngine.Random.value < starRainChance * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Special_BossErebos_StarRain>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossErebos_StarRain>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossErebos_StarRain>();
			}
			else if (UnityEngine.Random.value < rippleChance * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Special_BossErebos_Ripple>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossErebos_Ripple>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossErebos_Ripple>();
			}
			else if (UnityEngine.Random.value < spawnTormentorChance * context.deltaTime && normalizedHealth < 0.9f && AI.Helper_CanBeCast<At_Mon_Special_BossErebos_SpawnTormentor>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossErebos_SpawnTormentor>();
			}
			else if (UnityEngine.Random.value < spawnMeteorChance * context.deltaTime && AI.Helper_CanBeCast<At_Mon_Special_BossErebos_Meteor>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossErebos_Meteor>();
			}
			else if (_isPhaseChanged && UnityEngine.Random.value < antiGravityChance * context.deltaTime && normalizedHealth < 0.4f && AI.Helper_CanBeCast<At_Mon_Special_BossErebos_AntiGravity>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossErebos_AntiGravity>();
			}
			else
			{
				AI.Helper_ChaseTarget();
			}
		}
	}

	public override Type GetUniqueReward()
	{
		return typeof(Gem_U_LastStarlight);
	}

	private void MirrorProcessed()
	{
	}
}
