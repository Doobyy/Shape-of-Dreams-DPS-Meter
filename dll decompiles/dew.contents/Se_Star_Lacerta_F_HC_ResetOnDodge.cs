using System;
using Mirror;
using UnityEngine;

public class Se_Star_Lacerta_F_HC_ResetOnDodge : StarEffect
{
	public float addedCooldownTime = 2.5f;

	public GameObject fxReload;

	public override Type heroType => typeof(Hero_Lacerta);

	public override Type skillType => typeof(St_Q_HandCannon);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSkillBonusAll(new SkillBonus
			{
				cooldownOffset = addedCooldownTime
			});
			hero.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(OnSkillUse);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(OnSkillUse);
		}
	}

	private void OnSkillUse(EventInfoSkillUse obj)
	{
		if (obj.type == HeroSkillLocation.Movement && !((UnityEngine.Object)(object)skill == null))
		{
			ResetCooldown(skill);
			FxPlayNewNetworked(fxReload, hero);
		}
	}

	private void MirrorProcessed()
	{
	}
}
