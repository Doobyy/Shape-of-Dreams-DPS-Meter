using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_L_SpeedAfterDodge : StarEffect
{
	public StarScalingValue speedAmount;

	public float speedDuration;

	private ActorRef<StatusEffect> _prev;

	public override Type heroType => typeof(Hero_Lacerta);

	public override bool isMovementSkillType => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_prev = null;
			hero.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
		}
	}

	private void ClientHeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		if (obj.type == HeroSkillLocation.Movement)
		{
			if (!_prev.IsNullOrInactive())
			{
				_prev.Get().Destroy();
			}
			_prev = CreateBasicEffect(hero, new SpeedEffect
			{
				strength = GetValue(speedAmount)
			}, speedDuration);
		}
	}

	private void MirrorProcessed()
	{
	}
}
