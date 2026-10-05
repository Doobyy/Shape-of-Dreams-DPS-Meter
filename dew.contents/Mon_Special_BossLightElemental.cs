using System;
using System.Collections.Generic;
using UnityEngine;

public class Mon_Special_BossLightElemental : BossMonster, IPrewarmMonsterContributor
{
	public Transform[] scaleTransforms;

	public Vector3[] localScales;

	public Transform[] translateTransforms;

	public Vector3[] localPositions;

	public bool beamAtkEnabled;

	public bool summonEnabled;

	public float startMainSkillDelayTime;

	public float mainSkillIntervalTime;

	public bool lightningEnabled;

	public float lightningHpThreshold;

	public bool beamBarrageEnabled;

	public float beamBarrageHpThreshold;

	private float _lastMainSkillUseTime;

	private float _skillChanceMultiplier;

	private Vector3[] _baseTranslateLocalPositions;

	public void ContributeMonsterPrewarm(Dictionary<Monster, int> counts, int instanceCount)
	{
		if (!summonEnabled)
		{
			return;
		}
		Ai_Mon_Special_BossLightElemental_Summon byType = DewResources.GetByType<Ai_Mon_Special_BossLightElemental_Summon>(default(ResourceLoadSettings));
		if ((UnityEngine.Object)(object)byType == null || byType.entries == null || byType.entries.Length == 0)
		{
			return;
		}
		int num = ((DewPlayer.gamePlayers == null) ? 1 : DewPlayer.gamePlayers.Count);
		int num2 = (byType.entries.Length + num + byType.entries.Length - 1) / byType.entries.Length;
		Entity[] entries = byType.entries;
		for (int i = 0; i < entries.Length; i++)
		{
			if (entries[i] is Monster key)
			{
				counts.TryGetValue(key, out var value);
				counts[key] = value + num2 * 8 * instanceCount;
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (_baseTranslateLocalPositions == null && translateTransforms != null)
		{
			_baseTranslateLocalPositions = new Vector3[translateTransforms.Length];
			for (int i = 0; i < translateTransforms.Length; i++)
			{
				_baseTranslateLocalPositions[i] = translateTransforms[i].localPosition;
			}
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (_baseTranslateLocalPositions != null)
		{
			for (int i = 0; i < translateTransforms.Length; i++)
			{
				translateTransforms[i].localPosition = _baseTranslateLocalPositions[i];
			}
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity, "boss_unstoppable");
		_lastMainSkillUseTime = Time.time - mainSkillIntervalTime + startMainSkillDelayTime;
		_skillChanceMultiplier = NetworkedManagerBase<GameManager>.instance.GetSpecialSkillChanceMultiplier();
	}

	private void LateUpdate()
	{
		for (int i = 0; i < scaleTransforms.Length; i++)
		{
			scaleTransforms[i].localScale = localScales[i];
		}
		for (int j = 0; j < translateTransforms.Length; j++)
		{
			translateTransforms[j].localPosition += localPositions[j];
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if ((UnityEngine.Object)(object)context.targetEnemy == null)
		{
			return;
		}
		if (summonEnabled && AI.Helper_CanBeCast<At_Mon_Special_BossLightElemental_Summon>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossLightElemental_Summon>())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Special_BossLightElemental_Summon>();
			return;
		}
		if (Time.time - _lastMainSkillUseTime > mainSkillIntervalTime - 12f * _skillChanceMultiplier)
		{
			float num = currentHealth / maxHealth;
			if (UnityEngine.Random.value < 0.75f && beamBarrageEnabled && num < beamBarrageHpThreshold + 0.1f * _skillChanceMultiplier && AI.Helper_CanBeCast<At_Mon_Special_BossLightElemental_BeamBarrage>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossLightElemental_BeamBarrage>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossLightElemental_BeamBarrage>();
				_lastMainSkillUseTime = Time.time;
				return;
			}
			if (lightningEnabled && num < lightningHpThreshold && AI.Helper_CanBeCast<At_Mon_Special_BossLightElemental_Lightning>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossLightElemental_Lightning>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Special_BossLightElemental_Lightning>();
				_lastMainSkillUseTime = Time.time;
				return;
			}
		}
		if (beamAtkEnabled && AI.Helper_CanBeCast<At_Mon_Special_BossLightElemental_BeamAtk>() && AI.Helper_IsTargetInRangeOfAttack())
		{
			AI.Helper_CastAbilityAuto<At_Mon_Special_BossLightElemental_BeamAtk>();
		}
	}

	public override Type GetUniqueReward()
	{
		return typeof(St_U_WorldCracker);
	}

	private void MirrorProcessed()
	{
	}
}
