using System;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_L_DodgeCooldownOnKillOrAttack : StarEffect
{
	public StarScalingValue cooldownReductionRatio;

	public override Type heroType => typeof(Hero_Husk);

	public override bool isMovementSkillType => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
			hero.EntityEvent_OnAttackHit += new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
			hero.EntityEvent_OnAttackHit -= new Action<EventInfoAttackHit>(EntityEventOnAttackHit);
		}
	}

	private void EntityEventOnAttackHit(EventInfoAttackHit obj)
	{
		if (obj.victim.IsAnyBoss() && !((UnityEngine.Object)(object)skill == null))
		{
			ApplyCooldownReductionByRatio(skill, GetValue(cooldownReductionRatio), ignoreCanReceiveCooldown: true);
		}
	}

	private void ClientHeroEventOnKillOrAssist(EventInfoKill obj)
	{
		if (!((UnityEngine.Object)(object)skill == null))
		{
			ApplyCooldownReductionByRatio(skill, GetValue(cooldownReductionRatio), ignoreCanReceiveCooldown: true);
		}
	}

	private void MirrorProcessed()
	{
	}
}
