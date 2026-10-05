using System;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_F_IN_MoreProjByAtkSpd : StarEffect
{
	public float cooldownPenalty = 1f;

	private SafeAction _cleanupHandle;

	public override Type heroType => typeof(Hero_Bismuth);

	public override Type skillType => typeof(St_QR_Innocence);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			UpdateCooldownPenalty();
			hero.Skill.ClientHeroEvent_OnSkillEquip += new Action<SkillTrigger>(UpdateCooldownPenalty);
			hero.Skill.ClientHeroEvent_OnSkillUnequip += new Action<SkillTrigger>(UpdateCooldownPenalty);
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void UpdateCooldownPenalty(SkillTrigger _)
	{
		UpdateCooldownPenalty();
	}

	private void UpdateCooldownPenalty()
	{
		if (_cleanupHandle != null)
		{
			_cleanupHandle.Invoke();
			_cleanupHandle = null;
		}
		_cleanupHandle = DoSkillBonusAll(new SkillBonus
		{
			cooldownOffset = (((Hero_Bismuth)hero).HasSameTravelerMemory() ? cooldownPenalty : 0f)
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.Skill.ClientHeroEvent_OnSkillEquip -= new Action<SkillTrigger>(UpdateCooldownPenalty);
			hero.Skill.ClientHeroEvent_OnSkillUnequip -= new Action<SkillTrigger>(UpdateCooldownPenalty);
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_QR_Innocence_Spawner ai_QR_Innocence_Spawner)
		{
			float f = 1f / hero.Ability.attackAbility.configs[0].cooldownTime * hero.Status.attackSpeedMultiplier;
			ai_QR_Innocence_Spawner.spawnCountOffset += Mathf.RoundToInt(f);
		}
	}

	private void MirrorProcessed()
	{
	}
}
