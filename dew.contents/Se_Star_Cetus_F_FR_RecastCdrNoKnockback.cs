using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_F_FR_RecastCdrNoKnockback : StarEffect
{
	public float reduceRatio = 0.5f;

	public override Type heroType => typeof(Hero_Cetus);

	public override Type skillType => typeof(St_R_FrozenFists);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		DoSkill((SkillTrigger s) =>
		{
			s.configs[1].cooldownTime *= 1f - reduceRatio;
			return () =>
			{
				s.configs[1].cooldownTime = DewResources.GetByType<St_R_FrozenFists>(default(ResourceLoadSettings)).configs[1].cooldownTime;
			};
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_R_FrozenFists_PunchDash ai_R_FrozenFists_PunchDash)
		{
			ai_R_FrozenFists_PunchDash.disableKnockback = true;
			ai_R_FrozenFists_PunchDash.knockupAmount = 0f;
		}
	}

	private void MirrorProcessed()
	{
	}
}
