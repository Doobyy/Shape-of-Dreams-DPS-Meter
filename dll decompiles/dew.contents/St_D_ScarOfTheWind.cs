using System;
using Mirror;
using UnityEngine;

public class St_D_ScarOfTheWind : SkillTrigger
{
	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			if (newOwner is Hero hero)
			{
				hero.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
			}
			newOwner.Control.ClientEvent_OnTeleport += new Action<Vector3, Vector3>(ClientEventOnTeleport);
			newOwner.Control.ClientEvent_OnDisplacementStarted += new Action<Displacement>(ClientEventOnDisplacementStarted);
		}
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			if (formerOwner is Hero hero)
			{
				hero.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
			}
			if ((bool)(UnityEngine.Object)(object)formerOwner)
			{
				formerOwner.Control.ClientEvent_OnTeleport -= new Action<Vector3, Vector3>(ClientEventOnTeleport);
				formerOwner.Control.ClientEvent_OnDisplacementStarted -= new Action<Displacement>(ClientEventOnDisplacementStarted);
			}
		}
	}

	private void ClientEventOnDisplacementStarted(Displacement obj)
	{
		if (!NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && ManagerBase<TransitionManager>.instance.state != TransitionManager.StateType.Loading && obj.isFriendly)
		{
			ResetCooldown(owner.Ability.attackAbility);
			if (owner.Status.TryGetStatusEffect<Se_D_ScarOfTheWind_NextAtkCrit>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_D_ScarOfTheWind_NextAtkCrit>(owner, new CastInfo(owner));
			}
		}
	}

	private void ClientEventOnTeleport(Vector3 arg1, Vector3 arg2)
	{
		if (!NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && ManagerBase<TransitionManager>.instance.state != TransitionManager.StateType.Loading)
		{
			ResetCooldown(owner.Ability.attackAbility);
			if (owner.Status.TryGetStatusEffect<Se_D_ScarOfTheWind_NextAtkCrit>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_D_ScarOfTheWind_NextAtkCrit>(owner, new CastInfo(owner));
			}
		}
	}

	private void ClientHeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		if (obj.type == HeroSkillLocation.Movement)
		{
			if (owner.Status.TryGetStatusEffect<Se_D_ScarOfTheWind_NextAtkDash>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_D_ScarOfTheWind_NextAtkDash>(owner, new CastInfo(owner));
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
