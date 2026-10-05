using System;
using Mirror;
using UnityEngine;

public class St_D_BurningFist : SkillTrigger
{
	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			((Hero)newOwner).ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
		}
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)formerOwner == null))
		{
			((Hero)formerOwner).ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
		}
	}

	private void ClientHeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		if (obj.type != HeroSkillLocation.Movement)
		{
			if (owner.Status.TryGetStatusEffect<Se_D_BurningFist_EmpowerAttack>(out var effect))
			{
				effect.Destroy();
			}
			CreateStatusEffect<Se_D_BurningFist_EmpowerAttack>(owner, new CastInfo(owner));
		}
	}

	private void MirrorProcessed()
	{
	}
}
