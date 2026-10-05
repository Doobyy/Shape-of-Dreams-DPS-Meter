using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Mon_Special_BossMaw : BossMonster
{
	[Serializable]
	public class BossMawSkillData
	{
		public AssetRef<AbilityTrigger> abilityTrigger;

		[NonSerialized]
		public AbilityTrigger triggerInstance;

		public float chance;

		public bool checkRange;

		public float customRange;
	}

	public List<BossMawSkillData> totalSkillList = new List<BossMawSkillData>();

	[Space(15f)]
	public int maxSkillCountToUse;

	public int phase0SkillCount;

	public int phase1SkillCount;

	public float phase1ChangeHpThreshold = 0.75f;

	public float phase2ChangeHpThreshold = 0.35f;

	public float shadowWalkChance = 0.4f;

	public GameObject fxRoar;

	private List<BossMawSkillData> _randomSkillList = new List<BossMawSkillData>();

	public override Type GetUniqueReward()
	{
		return typeof(St_U_BigChomp);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		skipBossSoulFlow = true;
		_randomSkillList.Clear();
		List<BossMawSkillData> list = DewPool.GetList(out ListReturnHandle<BossMawSkillData> handle);
		list.AddRange(totalSkillList);
		for (int i = 0; i < list.Count; i++)
		{
			BossMawSkillData value = list[i];
			int index = UnityEngine.Random.Range(i, list.Count);
			list[i] = list[index];
			list[index] = value;
		}
		int num = Mathf.Min(maxSkillCountToUse, list.Count);
		for (int j = 0; j < num; j++)
		{
			_randomSkillList.Add(list[j]);
		}
		foreach (BossMawSkillData randomSkill in _randomSkillList)
		{
			foreach (AbilityTrigger value2 in Ability.abilities.Values)
			{
				if (((object)randomSkill.abilityTrigger.lightAsset).GetType() == ((object)value2).GetType())
				{
					randomSkill.triggerInstance = value2;
					break;
				}
			}
		}
		handle.Return();
		CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
		CreateStatusEffect<Se_Mon_Special_BossMaw_ShieldConversion>(this, new CastInfo(this));
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(4f);
			if (ManagerBase<CameraManager>.instance.isPlayingCutscene)
			{
				FxPlayNetworked(fxRoar, this);
				Control.StartDaze(1.5f);
			}
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if ((UnityEngine.Object)(object)context.targetEnemy == null)
		{
			return;
		}
		int num = 0;
		if (normalizedHealth < phase2ChangeHpThreshold)
		{
			num = maxSkillCountToUse;
		}
		else
		{
			num = ((!(normalizedHealth < phase1ChangeHpThreshold)) ? phase0SkillCount : phase1SkillCount);
		}
		if (AI.Helper_CanBeCast<At_Mon_Special_BossMaw_ShadowWalk>() && AI.Helper_IsTargetInRange<At_Mon_Special_BossMaw_ShadowWalk>() && UnityEngine.Random.value < shadowWalkChance * context.deltaTime)
		{
			AI.Helper_CastAbilityAuto<At_Mon_Special_BossMaw_ShadowWalk>();
			return;
		}
		if (num > _randomSkillList.Count)
		{
			num = _randomSkillList.Count;
		}
		for (int i = 0; i < num; i++)
		{
			BossMawSkillData bossMawSkillData = _randomSkillList[i];
			if (!(UnityEngine.Random.value >= bossMawSkillData.chance * context.deltaTime) && AI.Helper_CanBeCast(bossMawSkillData.triggerInstance) && (!bossMawSkillData.checkRange || AI.Helper_IsTargetInRange(bossMawSkillData.triggerInstance)) && (!(bossMawSkillData.customRange > 0f) || !(Vector3.Distance(context.targetEnemy.GetAIAgentPosition(this), agentPosition) > bossMawSkillData.customRange)))
			{
				AI.Helper_CastAbilityAuto(bossMawSkillData.triggerInstance);
			}
		}
		AI.Helper_ChaseTarget();
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		if (((NetworkBehaviour)this).isServer)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(3.5f);
			Dew.CreateActor(Dew.GetPositionOnGround(agentPosition), null, null, (Shrine_CallOfTheRavenous shrine) =>
			{
				shrine.SetItemReward(GetUniqueReward(), GetUniqueRewardChance());
			});
		}
	}

	private void MirrorProcessed()
	{
	}
}
