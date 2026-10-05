using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_F_NW_LessCooldownLessAtkSpd : StarEffect
{
	public float cooldownReduction = 0.7f;

	public float hasteReduction = 0.7f;

	public override Type heroType => typeof(Hero_Nachia);

	public override Type skillType => typeof(St_R_NaturesWhisper);

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
		if (obj.instance is Se_R_NaturesWhisper_Buff se_R_NaturesWhisper_Buff)
		{
			se_R_NaturesWhisper_Buff.hasteAmount *= 1f - hasteReduction;
		}
	}

	private void MirrorProcessed()
	{
	}
}
