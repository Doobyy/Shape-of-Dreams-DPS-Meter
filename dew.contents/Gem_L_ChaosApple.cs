using System;
using System.Linq;
using Mirror;
using UnityEngine;

public class Gem_L_ChaosApple : Gem
{
	public ScalingValue castPerRoomCount;

	public Color skillOverlayColor;

	public GameObject fxActivate;

	private AbilityLockHandle _handle;

	[SaveVar(SaveVarFlags.Default)]
	private string _originalSkill;

	private int _noChargeRevertStrikes;

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			_handle = newOwner.Ability.GetNewAbilityLockHandle(shouldShowLockIcon: true);
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			}
			if (_handle != null)
			{
				_handle.Stop();
				_handle = null;
			}
		}
	}

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer && _originalSkill != null)
		{
			ApplyChaosToSkill(newSkill);
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			return;
		}
		if (_originalSkill != null && (UnityEngine.Object)(object)skill != null && skill.currentCharges[0] == 0)
		{
			_noChargeRevertStrikes++;
			if (_noChargeRevertStrikes > 10 && (skill.children.Count == 0 || skill.children.All((Actor c) => !(c is AbilityInstance) || c.IsNullOrInactive())) && skill.currentConfigIndex == 0)
			{
				RevertSkill(skill);
			}
		}
		else
		{
			_noChargeRevertStrikes = 0;
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			RevertSkill(oldSkill);
		}
	}

	private void RevertSkill(SkillTrigger oldSkill)
	{
		string originalSkill = _originalSkill;
		if (originalSkill != null && !oldSkill.IsNullOrInactive() && !owner.IsNullOrInactive())
		{
			_originalSkill = null;
			int level = oldSkill.level;
			oldSkill.Destroy();
			DewPersistence.GeneralData data = DewPersistence.FromJson<DewPersistence.GeneralData>(originalSkill);
			SkillTrigger originalSkill2 = DewPersistence.CreateActor<SkillTrigger>(data);
			originalSkill2.level = level;
			Dew.CallDelayed(() =>
			{
				if (!originalSkill2.IsNullOrInactive())
				{
					originalSkill2.SetCharge(0, 0);
				}
			});
			owner.Skill.EquipSkill(location.skill, originalSkill2, ignoreCanReplace: true);
		}
		_handle.UnlockAllMainSkillsEdit();
	}

	private void ApplyChaosToSkill(SkillTrigger newSkill)
	{
		int num = Mathf.RoundToInt(GetValue(castPerRoomCount));
		newSkill.configs[0].maxCharges = num;
		newSkill.configs[0].canReceiveCooldownReduction = false;
		newSkill.LockCooldown(0);
		if (!newSkill.IsNullOrInactive())
		{
			newSkill.specialOverlayColor = skillOverlayColor;
			newSkill.AddSkillBonus(new SkillBonus
			{
				addedCharge = num - newSkill.mainConfigOriginalCharge
			});
			newSkill.SetCharge(0, newSkill.configs[0].maxCharges);
			_handle.LockAbilityEdit(newSkill.abilityIndex);
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom obj)
	{
		if (!obj.isTraveling)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(() =>
		{
			if (isValid && !((UnityEngine.Object)(object)skill == null))
			{
				int level = skill.level;
				string text = null;
				if (_originalSkill == null)
				{
					text = DewPersistence.ToJson(new DewPersistence.GeneralData().Capture(skill));
				}
				else
				{
					text = _originalSkill;
					_originalSkill = null;
				}
				skill.Destroy();
				Rarity rarity = skill.rarity;
				if (rarity == Rarity.Character || rarity == Rarity.Identity)
				{
					rarity = NetworkedManagerBase<LootManager>.instance.SelectSkillRarity();
				}
				NetworkedManagerBase<LootManager>.instance.SelectSkillAndLevel(rarity, out var trigger, out var _);
				SkillTrigger newSkill = Dew.CreateSkillTrigger(trigger, owner.agentPosition, level, owner.owner);
				owner.Skill.EquipSkill(location.skill, newSkill, ignoreCanReplace: true);
				ApplyChaosToSkill(newSkill);
				NotifyUse();
				FxPlayNetworked(fxActivate, owner);
				if (text != null)
				{
					_originalSkill = text;
				}
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
