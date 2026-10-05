using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Se_E_CrimsonLance_BossDrop : StatusEffect
{
	public Transform adjustedTransform;

	protected override void OnCreate()
	{
		base.OnCreate();
		adjustedTransform.rotation = info.rotation;
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		adjustedTransform.rotation = info.rotation;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			LockDestroy();
			yield return new WaitForSeconds(UnityEngine.Random.Range(0.4f, 0.8f));
			UnlockDestroy();
			if (!info.caster.IsNullInactiveDeadOrKnockedOut() && ManagerBase<TransitionManager>.instance.state != TransitionManager.StateType.Loading && victim.IsNullInactiveDeadOrKnockedOut() && victim is BossMonster bossMonster)
			{
				Type type = bossMonster.GetUniqueReward();
				if (type == null)
				{
					type = typeof(St_E_CrimsonLance);
				}
				UnityEngine.Object byType = DewResources.GetByType(type);
				Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(victim.position + (info.caster.position - victim.position).normalized * 3f, 1.25f);
				if (byType is SkillTrigger skillTrigger)
				{
					NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(skillTrigger.rarity, out var _, out var level);
					if (skillTrigger is St_E_CrimsonLance)
					{
						level = skillLevel;
					}
					SkillTrigger skill2 = Dew.CreateSkillTrigger(skillTrigger, goodRewardPosition, level, info.caster.owner);
					if (bossMonster is Mon_Primus_BossPrimusAeron && info.caster is Hero hero)
					{
						if ((UnityEngine.Object)(object)hero.Skill.Q == null)
						{
							hero.Skill.EquipSkill(HeroSkillLocation.Q, skill2, ignoreCanReplace: true);
						}
						else if ((UnityEngine.Object)(object)hero.Skill.W == null)
						{
							hero.Skill.EquipSkill(HeroSkillLocation.W, skill2, ignoreCanReplace: true);
						}
						else if ((UnityEngine.Object)(object)hero.Skill.E == null)
						{
							hero.Skill.EquipSkill(HeroSkillLocation.E, skill2, ignoreCanReplace: true);
						}
						else if ((UnityEngine.Object)(object)hero.Skill.R == null)
						{
							hero.Skill.EquipSkill(HeroSkillLocation.R, skill2, ignoreCanReplace: true);
						}
					}
				}
				else if (byType is Gem gem)
				{
					int quality = NetworkedManagerBase<LootManager>.instance.SelectGemQuality(gem.rarity);
					Dew.CreateGem(gem, goodRewardPosition, quality, info.caster.owner);
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
