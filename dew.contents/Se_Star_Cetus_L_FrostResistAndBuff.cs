using System;
using Mirror;
using UnityEngine;

public class Se_Star_Cetus_L_FrostResistAndBuff : StarEffect
{
	public StarScalingValue reductionFromCold;

	public StarScalingValue buffAmp;

	private StatBonus _bonus;

	public override Type heroType => typeof(Hero_Cetus);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			hero.takenDamageProcessor.Add(Process);
			hero.ClientEntityEvent_OnStatusEffectAdded += new Action<EventInfoStatusEffect>(CheckHasColdEffect);
			hero.ClientEntityEvent_OnStatusEffectRemoved += new Action<EventInfoStatusEffect>(CheckHasColdEffect);
			_bonus = DoStatBonus();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (bool)(UnityEngine.Object)(object)hero)
		{
			hero.takenDamageProcessor.Remove(Process);
			hero.ClientEntityEvent_OnStatusEffectAdded -= new Action<EventInfoStatusEffect>(CheckHasColdEffect);
			hero.ClientEntityEvent_OnStatusEffectRemoved -= new Action<EventInfoStatusEffect>(CheckHasColdEffect);
		}
	}

	private void CheckHasColdEffect(EventInfoStatusEffect _)
	{
		Dew.CallDelayed(() =>
		{
			if (isActive && !((UnityEngine.Object)(object)hero == null))
			{
				Se_Elm_Cold effect;
				if (!hero.Status.hasCold)
				{
					_bonus.attackSpeedPercentage = 0f;
					_bonus.movementSpeedPercentage = 0f;
				}
				else if (victim.Status.TryGetStatusEffect<Se_Elm_Cold>(out effect))
				{
					effect.StopAllBasicEffects();
					float value = GetValue(buffAmp);
					_bonus.attackSpeedPercentage = value * 100f;
					_bonus.movementSpeedPercentage = value * 100f;
				}
			}
		});
	}

	private void Process(ref DamageData data, Actor from, Entity to)
	{
		if (!data.IsAmountModifiedBy(this) && data.elemental == ElementalType.Cold)
		{
			data.ApplyReduction(GetValue(reductionFromCold));
			data.SetAmountModifiedBy(this);
		}
	}

	private void MirrorProcessed()
	{
	}
}
