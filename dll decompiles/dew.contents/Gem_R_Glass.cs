using System;
using Mirror;
using UnityEngine;

public class Gem_R_Glass : Gem
{
	public ScalingValue healByShieldRatio;

	public ScalingValue healAmpPerColdSkill;

	public float coldSkillCacheInterval = 2f;

	public GameObject fxHeal;

	private float _coldSkillCacheTime = float.NegativeInfinity;

	private int _cachedColdSkillCount;

	private float _cachedHealAmount;

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnTakeShield += new Action<EventInfoShield>(OnTakeShield);
			_coldSkillCacheTime = float.NegativeInfinity;
			_cachedHealAmount = 0f;
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)oldOwner == null))
		{
			oldOwner.EntityEvent_OnTakeShield -= new Action<EventInfoShield>(OnTakeShield);
			_cachedHealAmount = 0f;
		}
	}

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.dealtHealProcessor.Add(AmpHeal);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer && !oldSkill.IsNullOrInactive())
		{
			oldSkill.dealtHealProcessor.Remove(AmpHeal);
		}
	}

	private void OnTakeShield(EventInfoShield info)
	{
		if (owner.isInCombat && isValid && !info.chain.DidReact(this))
		{
			_cachedHealAmount += info.finalAmount * GetValue(healByShieldRatio);
			if (_cachedHealAmount > 1f)
			{
				Heal(_cachedHealAmount).Dispatch(owner, info.chain.New(this));
				FxPlayNewNetworked(fxHeal, owner);
				_cachedHealAmount = 0f;
				NotifyUse();
			}
		}
	}

	private void AmpHeal(ref HealData data, Actor actor, Entity target)
	{
		if (!data.IsAmountModifiedBy(this))
		{
			CalcColdSkillCount();
			if (_cachedColdSkillCount > 0)
			{
				data.ApplyAmplification(GetValue(healAmpPerColdSkill) * (float)_cachedColdSkillCount);
				data.SetAmountModifiedBy(this);
			}
		}
	}

	private void CalcColdSkillCount()
	{
		if (!(Time.time - _coldSkillCacheTime < coldSkillCacheInterval))
		{
			_coldSkillCacheTime = Time.time;
			_cachedColdSkillCount = 0;
			HeroSkill heroSkill = owner.Skill;
			CheckSkill(heroSkill.Q);
			CheckSkill(heroSkill.W);
			CheckSkill(heroSkill.E);
			CheckSkill(heroSkill.R);
			CheckSkill(heroSkill.Identity);
		}
		void CheckSkill(SkillTrigger s)
		{
			if (!s.IsNullOrInactive() && (s.tags & DescriptionTags.Cold) != 0)
			{
				_cachedColdSkillCount++;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
