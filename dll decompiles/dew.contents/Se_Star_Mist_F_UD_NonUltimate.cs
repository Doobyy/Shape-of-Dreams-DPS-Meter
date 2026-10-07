using System;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_F_UD_NonUltimate : StarEffect
{
	public float cooldownTime = 15f;

	public float effectReduction = 0.66f;

	public override Type heroType => typeof(Hero_Mist);

	public override Type skillType => typeof(St_R_UnbreakableDetermination);

	protected override void OnCreate()
	{
		base.OnCreate();
		DoSkill((SkillTrigger s) =>
		{
			s.configs[0].cooldownTime = cooldownTime;
			s.type = SkillType.Normal;
			return () =>
			{
				s.configs[0].cooldownTime = DewResources.GetByType<St_R_UnbreakableDetermination>(default(ResourceLoadSettings)).configs[0].cooldownTime;
				s.type = SkillType.Ultimate;
			};
		});
		if (((NetworkBehaviour)this).isServer)
		{
			victim.EntityEvent_OnCastCompleteBeforePrepare += new Action<EventInfoCast>(OnCastCompleteBeforePrepare);
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(OnAbilityInstanceBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.EntityEvent_OnCastCompleteBeforePrepare -= new Action<EventInfoCast>(OnCastCompleteBeforePrepare);
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(OnAbilityInstanceBeforePrepare);
		}
	}

	private void OnCastCompleteBeforePrepare(EventInfoCast obj)
	{
		if (obj.instance is Se_R_UnbreakableDetermination se_R_UnbreakableDetermination)
		{
			se_R_UnbreakableDetermination.skipExplosion = true;
			se_R_UnbreakableDetermination.invulnerableDuration = 0f;
			se_R_UnbreakableDetermination.initDuration *= 1f - effectReduction;
		}
	}

	private void OnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_R_UnbreakableDetermination_SubExplosion ai_R_UnbreakableDetermination_SubExplosion)
		{
			ai_R_UnbreakableDetermination_SubExplosion.dmgFactor *= 1f - effectReduction;
		}
	}

	private void MirrorProcessed()
	{
	}
}
