using System;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_F_EI_DashNoProjectile : StarEffect
{
	public override Type heroType => typeof(Hero_Yubar);

	public override Type skillType => typeof(St_Q_EtherealInfluence);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoSkill((SkillTrigger s) =>
		{
			TriggerConfig original = s.configs[0];
			s.configs[0] = s.configs[0].Clone();
			s.configs[0].overrideRotationDuration = 0.75f;
			s.configs[0].channel.duration = 0f;
			return () =>
			{
				s.configs[0] = original;
			};
		});
		hero.EntityEvent_OnCastCompleteBeforePrepare += new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
		hero.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(OnAbilityCreated);
	}

	private void EntityEventOnCastCompleteBeforePrepare(EventInfoCast obj)
	{
		if (obj.instance is Ai_Q_EtherealInfluence ai_Q_EtherealInfluence)
		{
			ai_Q_EtherealInfluence.isDashMode = true;
		}
	}

	private void OnAbilityCreated(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_EtherealInfluence ai_Q_EtherealInfluence)
		{
			ai_Q_EtherealInfluence.CreateAbilityInstance<Ai_Star_Yubar_F_EI_DashNoProjectile_Dash>(ai_Q_EtherealInfluence.info.caster.position, ai_Q_EtherealInfluence.info.caster.rotation, new CastInfo(ai_Q_EtherealInfluence.info.caster, ai_Q_EtherealInfluence.info.angle));
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)hero == null))
		{
			hero.EntityEvent_OnCastCompleteBeforePrepare -= new Action<EventInfoCast>(EntityEventOnCastCompleteBeforePrepare);
			hero.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(OnAbilityCreated);
		}
	}

	private void MirrorProcessed()
	{
	}
}
