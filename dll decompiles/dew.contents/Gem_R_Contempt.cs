using Mirror;
using UnityEngine;

public class Gem_R_Contempt : Gem
{
	public ScalingValue amp;

	public float maxAmpThreshold;

	public float critStrengthThreshold;

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
		if (owner.CheckEnemyOrNeutral(target) && !data.IsAmountModifiedBy(this))
		{
			float num = Mathf.Clamp01(1f - (target.normalizedHealth - maxAmpThreshold) / (1f - maxAmpThreshold));
			if (num > critStrengthThreshold)
			{
				data.SetAttr(DamageAttribute.IsCrit);
			}
			data.ApplyAmplification(Mathf.Lerp(0f, GetValue(amp), num));
			data.SetAmountModifiedBy(this);
			NotifyUse();
		}
	}

	private void MirrorProcessed()
	{
	}
}
