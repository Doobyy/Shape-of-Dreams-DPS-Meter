using Mirror;

public class Gem_E_Apathy : Gem
{
	public float reducedDamageRatio;

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			owner.dealtDamageProcessor.Add(Processor);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer && !owner.IsNullOrInactive())
		{
			owner.dealtDamageProcessor.Remove(Processor);
			if (owner.Status.TryGetStatusEffect<Se_Gem_E_Apathy_Armor>(out var effect))
			{
				effect.Destroy();
			}
		}
	}

	protected override void OnDealDamage(EventInfoDamage info)
	{
		base.OnDealDamage(info);
		if (owner.CheckEnemyOrNeutral(info.victim) && IsReady() && info.damage.elemental == ElementalType.Cold)
		{
			NotifyUse();
			if (owner.Status.TryGetStatusEffect<Se_Gem_E_Apathy_Armor>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_Gem_E_Apathy_Armor>(owner, new CastInfo(owner));
			}
		}
	}

	private void Processor(ref DamageData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			data.SetAmountModifiedBy(this);
			data.ApplyReduction(reducedDamageRatio);
		}
	}

	private void MirrorProcessed()
	{
	}
}
