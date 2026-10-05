using System;
using Mirror;
using UnityEngine;

public class Se_Star_Aurena_F_RE_CooldownButNoHeal : StarEffect
{
	public float cooldownReduction = 0.7f;

	public override Type heroType => typeof(Hero_Aurena);

	public override Type skillType => typeof(St_Q_Reduction);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)skill != null)
			{
				DoSkillBonusAll(new SkillBonus
				{
					cooldownMultiplier = 1f - cooldownReduction
				});
			}
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
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

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_Reduction_Projectile { collisionTargets: var collisionTargets })
		{
			collisionTargets.targets &= ~EntityRelation.Ally;
		}
	}

	private void MirrorProcessed()
	{
	}
}
