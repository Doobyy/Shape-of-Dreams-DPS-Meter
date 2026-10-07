using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_F_MP_StrongerFenrirNoCast : StarEffect
{
	public float attackAmp = 0.3f;

	public float armorAmount = 60f;

	public override Type heroType => typeof(Hero_Nachia);

	public override Type skillType => typeof(St_Q_MoonlightPact);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)skill != null)
			{
				DoSkillBonusAll(new SkillBonus
				{
					addedCharge = -999
				});
			}
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			hero.ActorEvent_OnSpawnSummon += new Action<EventInfoSummon>(ActorEventOnSpawnSummon);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			hero.ActorEvent_OnSpawnSummon -= new Action<EventInfoSummon>(ActorEventOnSpawnSummon);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_MoonlightPact_Fenrir_Atk ai_Q_MoonlightPact_Fenrir_Atk)
		{
			ai_Q_MoonlightPact_Fenrir_Atk.dmgFactor *= 1f + attackAmp;
		}
	}

	private void ActorEventOnSpawnSummon(EventInfoSummon obj)
	{
		if (obj.summon is Sum_Q_MoonlightPact_Fenrir sum_Q_MoonlightPact_Fenrir)
		{
			CreateBasicEffect(sum_Q_MoonlightPact_Fenrir, new ArmorBoostEffect
			{
				strength = armorAmount
			}, float.PositiveInfinity);
		}
	}

	private void MirrorProcessed()
	{
	}
}
