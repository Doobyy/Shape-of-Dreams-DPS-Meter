using System;
using Mirror;
using UnityEngine;

public class Gem_C_Love : Gem
{
	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.ActorEvent_OnGiveShield += new Action<EventInfoShield>(ActorEventOnGiveShield);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldSkill != null)
		{
			oldSkill.ActorEvent_OnGiveShield -= new Action<EventInfoShield>(ActorEventOnGiveShield);
		}
	}

	private void ActorEventOnGiveShield(EventInfoShield obj)
	{
		ApplyEffect(obj.target);
	}

	protected override void OnDoHeal(EventInfoHeal obj)
	{
		base.OnDoHeal(obj);
		ApplyEffect(obj.target);
	}

	private void ApplyEffect(Entity target)
	{
		EntityRelation relation = owner.GetRelation(target);
		if (relation == EntityRelation.Ally || relation == EntityRelation.Self)
		{
			Se_Gem_C_Love se_Gem_C_Love = target.Status.FindStatusEffect((Se_Gem_C_Love se) => (UnityEngine.Object)(object)se.info.caster == (UnityEngine.Object)(object)owner);
			if ((UnityEngine.Object)(object)se_Gem_C_Love != null)
			{
				se_Gem_C_Love.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_Gem_C_Love>(target, new CastInfo(owner));
			}
		}
		NotifyUse();
	}

	private void MirrorProcessed()
	{
	}
}
