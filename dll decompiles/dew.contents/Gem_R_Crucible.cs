using System;
using Mirror;
using UnityEngine;

public class Gem_R_Crucible : Gem
{
	public float critChanceBonus = 0.2f;

	public ScalingValue ampPerStack;

	public ScalingValue maxAmp;

	public GameObject fxCharge;

	public GameObject fxChargeEnd;

	private int _stacks;

	private int MaxStacks => Mathf.Max(1, Mathf.RoundToInt(GetValue(maxAmp) / GetValue(ampPerStack)));

	private float CurrentChargeAmp => Mathf.Min((float)_stacks * GetValue(ampPerStack), GetValue(maxAmp));

	public override void OnEquipGem(Hero newOwner)
	{
		base.OnEquipGem(newOwner);
		if (((NetworkBehaviour)this).isServer)
		{
			newOwner.EntityEvent_OnAttackHit += new Action<EventInfoAttackHit>(OnAttackHit);
			statBonus.critChanceFlat = critChanceBonus;
		}
	}

	public override void OnUnequipGem(Hero oldOwner)
	{
		base.OnUnequipGem(oldOwner);
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)oldOwner != null)
		{
			oldOwner.EntityEvent_OnAttackHit -= new Action<EventInfoAttackHit>(OnAttackHit);
		}
	}

	private void OnAttackHit(EventInfoAttackHit obj)
	{
		if (isValid && obj.isCrit && _stacks < MaxStacks)
		{
			_stacks = Mathf.Min(_stacks + 1, MaxStacks);
			numberDisplay = Mathf.RoundToInt(CurrentChargeAmp * 100f);
			FxPlayNewNetworked((_stacks >= MaxStacks) ? fxChargeEnd : fxCharge, owner);
			NotifyUse();
		}
	}

	protected override void OnQualityChange(int oldQuality, int newQuality)
	{
		base.OnQualityChange(oldQuality, newQuality);
		if (((NetworkBehaviour)this).isServer)
		{
			numberDisplay = Mathf.RoundToInt(CurrentChargeAmp * 100f);
		}
	}

	protected override void OnCastCompleteBeforePrepare(EventInfoCast info)
	{
		base.OnCastCompleteBeforePrepare(info);
		if (_stacks == 0)
		{
			return;
		}
		float amp = CurrentChargeAmp;
		info.instance.dealtDamageProcessor.Add(delegate(ref DamageData data, Actor a, Entity t)
		{
			if (owner.CheckEnemyOrNeutral(t) && !data.IsAmountModifiedBy(this))
			{
				data.ApplyAmplification(amp);
				data.SetAttr(DamageAttribute.IsCrit);
				data.SetAmountModifiedBy(this);
			}
		});
		info.instance.dealtHealProcessor.Add(delegate(ref HealData data, Actor a, Entity t)
		{
			if (!data.IsAmountModifiedBy(this))
			{
				data.ApplyAmplification(amp);
				data.SetCrit();
				data.SetAmountModifiedBy(this);
			}
		});
		_stacks = 0;
		numberDisplay = 0;
	}

	private void MirrorProcessed()
	{
	}
}
