using System;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_F_VH_ResetOnDodge : StarEffect
{
	public int targetLimitCount = 2;

	public override Type heroType => typeof(Hero_Bismuth);

	public override Type skillType => typeof(St_QR_ValiantHeart);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)hero != null)
			{
				hero.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
			}
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			}
		}
	}

	private void ClientHeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		if (obj.type == HeroSkillLocation.Movement)
		{
			SkillTrigger[] array = skills;
			foreach (SkillTrigger trigger in array)
			{
				ResetCooldown(trigger);
			}
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_QR_ValiantHeart ai_QR_ValiantHeart)
		{
			ai_QR_ValiantHeart.targetLimitCount = targetLimitCount;
		}
	}

	private void MirrorProcessed()
	{
	}
}
