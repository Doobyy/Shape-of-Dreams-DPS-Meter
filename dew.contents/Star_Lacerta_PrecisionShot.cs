using System;

public class Star_Lacerta_PrecisionShot : DewHeroStarItemOld
{
	public static readonly float BonusChargeSpeed = 0.4f;

	public override Type heroType => typeof(Hero_Lacerta);

	public override Type affectedSkill => typeof(St_R_PrecisionShot);

	public override bool ShouldInitInGame()
	{
		if (base.ShouldInitInGame())
		{
			return isServer;
		}
		return false;
	}

	public override void OnStartInGame()
	{
		base.OnStartInGame();
		hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_R_PrecisionShot { channel: var channel })
		{
			channel.chargeFullDuration /= 1f + BonusChargeSpeed;
		}
	}
}
