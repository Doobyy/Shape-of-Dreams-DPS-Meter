using System;
using Mirror;
using UnityEngine;

public class Se_Gem_U_SoulPrison_DeathInterrupt : StatusEffect
{
	public ScalingValue healPerQuality;

	public ScalingValue invulTime;

	public GameObject fxActivate;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		ActorEvent_OnDoHeal += (Action<EventInfoHeal>)((EventInfoHeal heal) =>
		{
			if (heal.discardedAmount > 0.01f)
			{
				GiveShield(victim, heal.discardedAmount, float.PositiveInfinity);
			}
		});
		DoDeathInterrupt((EventInfoKill _) =>
		{
			if ((UnityEngine.Object)(object)gem == null)
			{
				Destroy();
			}
			else
			{
				victim.Status.SetHealth(1f);
				if (!victim.Status.HasStatusEffect<Se_FatalHitProtection_Invulnerable>())
				{
					Heal(healPerQuality).ApplyRawMultiplier(gem.quality).Dispatch(victim);
					CreateStatusEffect(victim, (Se_FatalHitProtection_Invulnerable se) =>
					{
						se.timerCustomNameKey = "Gem_U_SoulPrison";
						se.duration = GetValue(invulTime);
					});
					FxPlayNetworked(fxActivate, victim);
					Dew.CallDelayed(() =>
					{
						Hero hero = (Hero)victim;
						if (hero.Skill.TryGetGemLocation(gem, out var location))
						{
							hero.Skill.UnequipGem(location, Vector3.zero);
							gem.Destroy();
						}
						Destroy();
					});
				}
			}
		}, 0);
	}

	private void MirrorProcessed()
	{
	}
}
