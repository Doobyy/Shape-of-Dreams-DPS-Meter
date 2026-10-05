using System;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_F_AS_NonUltimate : StarEffect
{
	public float cooldownTime = 14f;

	public float strengthReduction = 0.4f;

	public float newDuration = 4f;

	public override Type heroType => typeof(Hero_Husk);

	public override Type skillType => typeof(St_R_AnnihilationStance);

	protected override void OnCreate()
	{
		base.OnCreate();
		DoSkill((SkillTrigger s) =>
		{
			s.configs[0].cooldownTime = cooldownTime;
			s.type = SkillType.Normal;
			return () =>
			{
				s.configs[0].cooldownTime = DewResources.GetByType<St_R_AnnihilationStance>(default(ResourceLoadSettings)).configs[0].cooldownTime;
				s.type = SkillType.Ultimate;
			};
		});
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Se_R_AnnihilationStance se_R_AnnihilationStance)
		{
			se_R_AnnihilationStance.adPercentage *= 1f - strengthReduction;
			se_R_AnnihilationStance.duration = newDuration;
		}
		if (obj.instance is Ai_R_AnnihilationStance_Projectile ai_R_AnnihilationStance_Projectile)
		{
			ai_R_AnnihilationStance_Projectile.damage *= 1f - strengthReduction;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void MirrorProcessed()
	{
	}
}
