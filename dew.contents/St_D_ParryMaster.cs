using Mirror;
using UnityEngine;

public class St_D_ParryMaster : SkillTrigger
{
	public ScalingValue maxCharge;

	[SaveVar(SaveVarFlags.Default)]
	private string _originalSkill;

	private SkillBonus _bonus;

	public int clampedCharge => Mathf.Min(Mathf.RoundToInt(GetValue(maxCharge)), 3);

	protected override void OnEquip(Entity newOwner)
	{
		base.OnEquip(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			ApplyParry(newOwner);
		}
	}

	protected override void OnUnequip(Entity formerOwner)
	{
		base.OnUnequip(formerOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			RevertToDodge(formerOwner);
		}
	}

	private void ApplyParry(Entity newOwner)
	{
		SkillTrigger movement = owner.Skill.Movement;
		if (_originalSkill == null)
		{
			_originalSkill = DewPersistence.ToJson(new DewPersistence.GeneralData().Capture(movement));
			movement.Destroy();
			St_M_ParryMaster st_M_ParryMaster = Dew.CreateSkillTrigger<St_M_ParryMaster>(owner.agentPosition, level, owner.owner);
			_bonus = st_M_ParryMaster.AddSkillBonus(new SkillBonus
			{
				addedCharge = clampedCharge
			});
			owner.Skill.EquipSkill(HeroSkillLocation.Movement, st_M_ParryMaster, ignoreCanReplace: true);
		}
	}

	protected override void OnLevelChange(int oldLevel, int newLevel)
	{
		base.OnLevelChange(oldLevel, newLevel);
		if (((NetworkBehaviour)this).isServer && newLevel != 1 && owner.Skill.Movement is St_M_ParryMaster st_M_ParryMaster)
		{
			st_M_ParryMaster.level = newLevel;
			ApplyMaxCharge();
		}
	}

	private void ApplyMaxCharge()
	{
		if (_bonus == null)
		{
			_bonus = owner.Skill.Movement.AddSkillBonus(new SkillBonus
			{
				addedCharge = clampedCharge
			});
		}
		_bonus.addedCharge = clampedCharge;
	}

	private void RevertToDodge(Entity formerOwner)
	{
		string originalSkill = _originalSkill;
		if (_bonus != null)
		{
			_bonus.Stop();
			_bonus = null;
		}
		if (originalSkill == null || formerOwner.IsNullOrInactive() || !(formerOwner is Hero hero))
		{
			return;
		}
		_originalSkill = null;
		hero.Skill.Movement.Destroy();
		DewPersistence.GeneralData data = DewPersistence.FromJson<DewPersistence.GeneralData>(originalSkill);
		SkillTrigger originalSkill2 = DewPersistence.CreateActor<SkillTrigger>(data);
		Dew.CallDelayed(() =>
		{
			if (!originalSkill2.IsNullOrInactive())
			{
				originalSkill2.SetCharge(0, 0);
			}
		});
		hero.Skill.EquipSkill(HeroSkillLocation.Movement, originalSkill2, ignoreCanReplace: true);
	}

	private void MirrorProcessed()
	{
	}
}
