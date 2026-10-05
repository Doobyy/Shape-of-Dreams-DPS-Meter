using System;
using Mirror;
using UnityEngine;

public class Gem_R_Rigidity : Gem
{
	public ScalingValue shieldAmount;

	public float shieldDuration = 2f;

	public float coldDamageMultiplier = 2f;

	public float procCoefficientPower = 1.5f;

	public GameObject giveShieldEffect;

	private ActorRef<Se_GenericShield_Stacking> _shield;

	public override void OnEquipSkill(SkillTrigger newSkill)
	{
		base.OnEquipSkill(newSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			newSkill.ActorEvent_OnDealDamage += new Action<EventInfoDamage>(AddShield);
			_shield = CreateStatusEffect(owner, new CastInfo(owner), (Se_GenericShield_Stacking s) =>
			{
				s.timeout = shieldDuration;
			});
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)oldSkill != null)
			{
				oldSkill.ActorEvent_OnDealDamage -= new Action<EventInfoDamage>(AddShield);
			}
			if (!_shield.IsNullOrInactive())
			{
				_shield.Get().Destroy();
				_shield = null;
			}
		}
	}

	private void AddShield(EventInfoDamage obj)
	{
		if (owner.CheckEnemyOrNeutral(obj.victim) && !obj.chain.DidReact(this) && IsReady() && !_shield.IsNullOrInactive())
		{
			float num = GetValue(shieldAmount);
			if (obj.damage.elemental == ElementalType.Cold)
			{
				num *= coldDamageMultiplier;
			}
			_shield.Get().AddAmount(num * Mathf.Pow(obj.damage.procCoefficient, procCoefficientPower), obj.chain.New(this));
			FxPlayNewNetworked(giveShieldEffect, owner);
			NotifyUse();
		}
	}

	private void MirrorProcessed()
	{
	}
}
