using System;
using Mirror;
using UnityEngine;

public class St_D_IcyVeins : SkillTrigger
{
	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(ActorEventOnDealDamage);
		}
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)formerOwner)
		{
			formerOwner.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(ActorEventOnDealDamage);
			if (formerOwner.Status.TryGetStatusEffect<Se_D_IcyVeins>(out var effect))
			{
				effect.Destroy();
			}
		}
	}

	private void ActorEventOnDealDamage(EventInfoDamage obj)
	{
		if (obj.damage.elemental == ElementalType.Cold && !((UnityEngine.Object)(object)obj.actor == (UnityEngine.Object)(object)this) && !obj.actor.IsDescendantOf(this))
		{
			if (owner.Status.TryGetStatusEffect<Se_D_IcyVeins>(out var effect))
			{
				effect.AddStack();
			}
			else
			{
				effect = CreateStatusEffect<Se_D_IcyVeins>(owner, new CastInfo(owner));
			}
			effect.FxPlayNetworked(effect.fxAddStack, owner);
		}
	}

	private void MirrorProcessed()
	{
	}
}
