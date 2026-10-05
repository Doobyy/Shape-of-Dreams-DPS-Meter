using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Gem_E_Flexibility : Gem
{
	private struct PostponedDamage
	{
		public float damagePerTick;

		public int remainingTicks;
	}

	public int dmgTickCount;

	public float tickInterval;

	public ScalingValue dmgDivideAmount;

	public GameObject hitEffect;

	public float dmgReduceAmount;

	private float _lastCheckTime;

	private List<PostponedDamage> _postponedDamages = new List<PostponedDamage>();

	public float divMultiplier => 1f - 1f / (GetValue(dmgDivideAmount) + 1f);

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.takenDamageProcessor.Add(OnTakenDamage, 1000);
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && !((Object)(object)oldOwner == null))
		{
			oldOwner.takenDamageProcessor.Remove(OnTakenDamage);
			float num = 0f;
			for (int num2 = _postponedDamages.Count - 1; num2 >= 0; num2--)
			{
				PostponedDamage postponedDamage = _postponedDamages[num2];
				num += postponedDamage.damagePerTick * (float)postponedDamage.remainingTicks;
				_postponedDamages.RemoveAt(num2);
			}
			if (!(num <= 0f))
			{
				PureDamage(num).SetAttr(DamageAttribute.DamageOverTime).Dispatch(oldOwner);
				FxPlayNewNetworked(hitEffect, oldOwner);
			}
		}
	}

	private void OnTakenDamage(ref DamageData data, Actor actor, Entity target)
	{
		if (!isValid || data.HasAttr(DamageAttribute.DamageShieldOnly) || data.isBlockedByImmunity || target.Status.hasDamageImmunity || (Object)(object)skill == null || skill.currentCharges[0] >= skill.configs[0].maxCharges || (Object)(object)data.actor == (Object)(object)this)
		{
			return;
		}
		float currentAmount = data.currentAmount;
		data.ApplyRawMultiplier(1f - divMultiplier);
		float num = currentAmount * divMultiplier * dmgReduceAmount;
		if (num < 0.7f)
		{
			return;
		}
		float num2 = num / (float)dmgTickCount;
		if (_postponedDamages.Count != 0)
		{
			List<PostponedDamage> postponedDamages = _postponedDamages;
			if (postponedDamages[postponedDamages.Count - 1].remainingTicks == dmgTickCount)
			{
				List<PostponedDamage> postponedDamages2 = _postponedDamages;
				PostponedDamage value = postponedDamages2[postponedDamages2.Count - 1];
				value.damagePerTick += num2;
				List<PostponedDamage> postponedDamages3 = _postponedDamages;
				postponedDamages3[postponedDamages3.Count - 1] = value;
				return;
			}
		}
		_postponedDamages.Add(new PostponedDamage
		{
			damagePerTick = num2,
			remainingTicks = dmgTickCount
		});
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || _postponedDamages.Count <= 0)
		{
			return;
		}
		if (owner.IsNullInactiveDeadOrKnockedOut())
		{
			_postponedDamages.Clear();
		}
		else
		{
			if (!(Time.time - _lastCheckTime > tickInterval))
			{
				return;
			}
			_lastCheckTime = Time.time;
			float num = 0f;
			for (int num2 = _postponedDamages.Count - 1; num2 >= 0; num2--)
			{
				PostponedDamage value = _postponedDamages[num2];
				if (value.remainingTicks == 0)
				{
					_postponedDamages.RemoveAt(num2);
				}
				else
				{
					value.remainingTicks--;
					_postponedDamages[num2] = value;
					num += value.damagePerTick;
				}
			}
			DefaultDamage(num).SetAttr(DamageAttribute.DamageOverTime).Dispatch(owner);
			FxPlayNewNetworked(hitEffect, owner);
			NotifyUse();
		}
	}

	private void MirrorProcessed()
	{
	}
}
