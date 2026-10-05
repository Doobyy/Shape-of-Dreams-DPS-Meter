using System;
using Mirror;
using UnityEngine;

public class Se_D_CircleOfLife_Summon : StatusEffect
{
	public ScalingValue shareRatio;

	private StatBonus _bonus;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus = DoStatBonus();
			if (_bonus != null)
			{
				info.caster.Status.ClientEvent_OnFinalStatsChanged += new Action(SyncAttackSpeedBonus);
				SyncAttackSpeedBonus();
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)info.caster != null)
		{
			info.caster.Status.ClientEvent_OnFinalStatsChanged -= new Action(SyncAttackSpeedBonus);
		}
	}

	private void SyncAttackSpeedBonus()
	{
		float value = GetValue(shareRatio);
		_bonus.attackSpeedPercentage = Mathf.Max(0f, (info.caster.Status.attackSpeedMultiplier - 1f) * 100f) * value;
		_bonus.movementSpeedPercentage = Mathf.Max(0f, info.caster.Status.bonusStats.movementSpeedPercentage + info.caster.Status.totalSpeed) * value;
		_bonus.armorFlat = Mathf.Max(0f, info.caster.Status.armor * value);
	}

	private void MirrorProcessed()
	{
	}
}
