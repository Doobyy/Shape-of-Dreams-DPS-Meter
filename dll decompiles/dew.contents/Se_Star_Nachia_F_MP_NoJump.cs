using System;
using Mirror;
using UnityEngine;

public class Se_Star_Nachia_F_MP_NoJump : StarEffect
{
	public int addedCharges = 1;

	public GameObject fxOnFenrir;

	public override Type heroType => typeof(Hero_Nachia);

	public override Type skillType => typeof(St_Q_MoonlightPact);

	protected override void OnCreate()
	{
		base.OnCreate();
		if ((UnityEngine.Object)(object)skill != null)
		{
			skill.configs[0].castMethod.type = CastMethodType.None;
		}
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)skill != null)
			{
				DoSkillBonusAll(new SkillBonus
				{
					addedCharge = addedCharges
				});
			}
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((UnityEngine.Object)(object)skill != null)
		{
			skill.configs[0].castMethod.type = CastMethodType.Point;
		}
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_MoonlightPact ai_Q_MoonlightPact)
		{
			ai_Q_MoonlightPact.NetworknoJump = true;
			Summon fen = hero.summons.Find((Summon s) => s is Sum_Q_MoonlightPact_Fenrir);
			FxPlayNetworked(fxOnFenrir, fen);
			Dew.CallDelayed(() =>
			{
				fen.Control.StartDaze(0.15f);
			});
		}
		if (obj.instance is Ai_Q_MoonlightPact_Land ai_Q_MoonlightPact_Land)
		{
			ai_Q_MoonlightPact_Land.range.transform.localScale *= 1.2f;
		}
	}

	private void MirrorProcessed()
	{
	}
}
