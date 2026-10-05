using Mirror;
using UnityEngine;

public class Gem_U_EternalFlame : Gem
{
	public ScalingValue ampPerFireStack;

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.dealtDamageProcessor.Add(Processor);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer && (Object)(object)oldSkill != null)
		{
			oldSkill.dealtDamageProcessor.Remove(Processor);
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (isValid && !data.IsAmountModifiedBy(this) && owner.CheckEnemyOrNeutral(target) && target.Status.fireStack != 0)
		{
			data.SetAmountModifiedBy(this);
			if (target.Status.fireStack >= 5)
			{
				data.SetAttr(DamageAttribute.IsCrit);
			}
			data.ApplyAmplification((float)target.Status.fireStack * GetValue(ampPerFireStack));
		}
	}

	protected override void OnDealDamage(EventInfoDamage info)
	{
		base.OnDealDamage(info);
		if (info.damage.elemental == ElementalType.Fire && owner.CheckEnemyOrNeutral(info.victim))
		{
			Se_U_EternalFlame_Curse se_U_EternalFlame_Curse = info.victim.Status.FindStatusEffect((Se_U_EternalFlame_Curse se) => (Object)(object)se.parentActor == (Object)(object)this);
			if ((Object)(object)se_U_EternalFlame_Curse != null)
			{
				se_U_EternalFlame_Curse.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_U_EternalFlame_Curse>(info.victim, new CastInfo(owner));
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
