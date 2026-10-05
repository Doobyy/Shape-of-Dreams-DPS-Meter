using Mirror;
using UnityEngine;

public class Se_Mon_Special_BossMaw_Cataclysm : StatusEffect
{
	public float shieldAmount;

	public float duration;

	public float bonusApPercent;

	private StatBonus _bonus;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DestroyOnDeath(info.caster);
			GiveShield(victim, victim.Status.maxHealth * shieldAmount, duration);
			DoUnstoppable();
			_bonus = new StatBonus
			{
				abilityPowerPercentage = bonusApPercent
			};
			victim.Status.AddStatBonus(_bonus);
			SetTimer(duration);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (Object)(object)victim != null && _bonus != null)
		{
			victim.Status.RemoveStatBonus(_bonus);
		}
	}

	private void MirrorProcessed()
	{
	}
}
