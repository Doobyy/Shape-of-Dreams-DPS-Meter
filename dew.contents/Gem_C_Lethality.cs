using Mirror;
using UnityEngine;

public class Gem_C_Lethality : Gem
{
	public ScalingValue dmgAmp;

	public float healthThreshold;

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.dealtDamageProcessor.Add(Amplify);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer && (Object)(object)oldSkill != null)
		{
			oldSkill.dealtDamageProcessor.Remove(Amplify);
		}
	}

	private void Amplify(ref DamageData data, Actor actor, Entity target)
	{
		if (owner.CheckEnemyOrNeutral(target) && !data.IsAmountModifiedBy(this) && !(target.Status.currentHealth / target.Status.maxHealth < healthThreshold))
		{
			data.SetAttr(DamageAttribute.IsCrit);
			data.ApplyAmplification(GetValue(dmgAmp));
			data.SetAmountModifiedBy(this);
			NotifyUse();
		}
	}

	private void MirrorProcessed()
	{
	}
}
