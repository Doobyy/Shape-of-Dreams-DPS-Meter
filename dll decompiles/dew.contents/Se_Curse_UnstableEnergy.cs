using System;
using Mirror;
using UnityEngine;

public class Se_Curse_UnstableEnergy : CurseStatusEffect
{
	public float[] selfDamageHpRatio;

	public GameObject fxDamageSelf;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && victim is Hero hero)
		{
			hero.ClientHeroEvent_OnSkillUse += new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null && victim is Hero hero)
		{
			hero.ClientHeroEvent_OnSkillUse -= new Action<EventInfoSkillUse>(ClientHeroEventOnSkillUse);
		}
	}

	private void ClientHeroEventOnSkillUse(EventInfoSkillUse obj)
	{
		HeroSkillLocation type = obj.type;
		if (type != HeroSkillLocation.Movement && type != HeroSkillLocation.Identity)
		{
			PureDamage(GetValue(selfDamageHpRatio) * victim.maxHealth).SetAttr(DamageAttribute.IgnoreShield).Dispatch(victim);
			FxPlayNetworked(fxDamageSelf, victim);
		}
	}

	private void MirrorProcessed()
	{
	}
}
