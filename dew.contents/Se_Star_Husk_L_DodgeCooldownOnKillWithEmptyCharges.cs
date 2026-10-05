using System;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_L_DodgeCooldownOnKillWithEmptyCharges : StarEffect
{
	public StarScalingValue cooldownReductionRatio;

	public StarScalingValue cooldownPenalty;

	public override Type heroType => typeof(Hero_Husk);

	public override bool isMovementSkillType => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
			if ((UnityEngine.Object)(object)skill != null)
			{
				DoSkillBonusAll(new SkillBonus
				{
					cooldownOffset = GetValue(cooldownPenalty),
					ignoreReceiveCooldownReductionFlag = true
				});
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
		}
	}

	private void ClientHeroEventOnKillOrAssist(EventInfoKill obj)
	{
		if (!((UnityEngine.Object)(object)skill == null) && skill.currentCharges[0] <= 0)
		{
			ApplyCooldownReductionByRatio(skill, GetValue(cooldownReductionRatio), ignoreCanReceiveCooldown: true);
		}
	}

	private void MirrorProcessed()
	{
	}
}
