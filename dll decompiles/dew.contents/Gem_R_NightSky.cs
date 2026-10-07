using System;
using Mirror;
using UnityEngine;

public class Gem_R_NightSky : Gem
{
	public ScalingValue perHitAtkSpd;

	public ScalingValue maxAtkSpdNormal;

	public ScalingValue maxAtkSpdDark;

	public bool useProcCoefficient;

	public float duration;

	private ActorRef<Se_R_NightSky_AtkSpd> _atkSpd;

	private float _lastDamageTime;

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnAttackEffectTriggered += new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
		}
	}

	public override void OnUnequipSkill(SkillTrigger oldSkill)
	{
		base.OnUnequipSkill(oldSkill);
		if (((NetworkBehaviour)this).isServer)
		{
			if (!_atkSpd.IsNullOrInactive())
			{
				_atkSpd.Get().Destroy();
			}
			_atkSpd = null;
			numberDisplay = Mathf.RoundToInt(0f);
			ResetCooldown();
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			if (!_atkSpd.IsNullOrInactive())
			{
				_atkSpd.Get().Destroy();
			}
			_atkSpd = null;
			numberDisplay = Mathf.RoundToInt(0f);
			ResetCooldown();
			if ((UnityEngine.Object)(object)oldOwner != null)
			{
				oldOwner.EntityEvent_OnAttackEffectTriggered -= new Action<EventInfoAttackEffect>(EntityEventOnAttackEffectTriggered);
			}
		}
	}

	private void EntityEventOnAttackEffectTriggered(EventInfoAttackEffect obj)
	{
		if (!_atkSpd.IsNullOrInactive())
		{
			_lastDamageTime = Time.time;
			if (_atkSpd.Get().haste.strength > 0f)
			{
				StartCooldown();
			}
		}
	}

	protected override void OnDealDamage(EventInfoDamage info)
	{
		base.OnDealDamage(info);
		if (owner.CheckEnemyOrNeutral(info.victim) && isValid)
		{
			if (_atkSpd.IsNullOrInactive())
			{
				_atkSpd = CreateStatusEffectWithSource<Se_R_NightSky_AtkSpd>(info.actor, owner, new CastInfo(owner));
			}
			_lastDamageTime = Time.time;
			NotifyUse();
			StartCooldown();
			Se_R_NightSky_AtkSpd se_R_NightSky_AtkSpd = _atkSpd.Get();
			float strength = se_R_NightSky_AtkSpd.haste.strength;
			float num = ((info.damage.elemental == ElementalType.Dark) ? GetValue(maxAtkSpdDark) : GetValue(maxAtkSpdNormal));
			if (!(strength > num))
			{
				strength = Mathf.MoveTowards(strength, num, GetValue(perHitAtkSpd) * (useProcCoefficient ? info.damage.procCoefficient : 1f));
				se_R_NightSky_AtkSpd.haste.strength = strength;
				se_R_NightSky_AtkSpd.Network_effectStrength = strength / GetValue(maxAtkSpdDark);
				numberDisplay = Mathf.RoundToInt(se_R_NightSky_AtkSpd.haste.strength);
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (((NetworkBehaviour)this).isServer && !((UnityEngine.Object)(object)owner == null) && !_atkSpd.IsNullOrInactive() && _atkSpd.Get().haste != null && Time.time - _lastDamageTime > duration)
		{
			_atkSpd.Get().Destroy();
			_atkSpd = null;
			numberDisplay = Mathf.RoundToInt(0f);
		}
	}

	private void MirrorProcessed()
	{
	}
}
