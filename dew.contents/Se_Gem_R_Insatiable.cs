using Mirror;
using UnityEngine;

public class Se_Gem_R_Insatiable : StatusEffect
{
	public ScalingValue healAmount;

	public GameObject healEffect;

	public GameObject hitEffect;

	public float duration;

	public ScalingValue bonusAttackDamagePercentage;

	private StatBonus _bonus;

	public override bool reuseInRoom => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		_bonus = new StatBonus
		{
			attackDamagePercentage = GetValue(bonusAttackDamagePercentage)
		};
		victim.Status.AddStatBonus(_bonus);
		SetTimer(duration);
		gem.NotifyUse();
		DoAttackEmpower((EventInfoAttackEffect effect, int i) =>
		{
			if (!effect.chain.DidReact(this))
			{
				FxPlayNewNetworked(healEffect, victim);
				FxPlayNewNetworked(hitEffect, effect.victim);
				DoHeal(new HealData(GetValue(healAmount) * effect.strength), victim, effect.chain.New(this));
				gem.NotifyUse();
			}
		});
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && !((Object)(object)victim == null))
		{
			victim.Status.RemoveStatBonus(_bonus);
		}
	}

	private void MirrorProcessed()
	{
	}
}
