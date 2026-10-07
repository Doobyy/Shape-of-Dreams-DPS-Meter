using System;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_F_LA_DodgeResetOnRed : StarEffect
{
	public float cooldownPenalty = 1f;

	public override Type heroType => typeof(Hero_Husk);

	public override Type skillType => typeof(St_Q_Laceration);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)skill != null)
			{
				DoSkillBonusAll(new SkillBonus
				{
					cooldownOffset = cooldownPenalty
				});
			}
			hero.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
		}
	}

	private void ActorEventOnAbilityInstanceCreated(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_Laceration_Dash)
		{
			ResetCooldown(hero.Skill.Movement, ignoreCanReceiveCooldown: true);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
		}
	}

	private void MirrorProcessed()
	{
	}
}
