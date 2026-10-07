using System;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_F_CS_ColdDamage : StarEffect
{
	public float addedStunDuration = 1f;

	public float gainedArmorAmp = 0.5f;

	public override Type heroType => typeof(Hero_Vesper);

	public override Type skillType => typeof(St_Q_CruelSun);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			if ((UnityEngine.Object)(object)skill != null)
			{
				skill.tags &= ~DescriptionTags.Fire;
				skill.tags |= DescriptionTags.Cold;
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			}
			if ((UnityEngine.Object)(object)skill != null)
			{
				skill.tags |= DescriptionTags.Fire;
				skill.tags &= ~DescriptionTags.Cold;
			}
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_CruelSun ai_Q_CruelSun)
		{
			ai_Q_CruelSun.NetworkdoColdDamage = true;
			ai_Q_CruelSun.stunDurationOffset += addedStunDuration;
		}
		if (obj.instance is Se_Q_CruelSun_Armored se_Q_CruelSun_Armored)
		{
			se_Q_CruelSun_Armored.armorAmount *= 1f + gainedArmorAmp;
		}
	}

	private void MirrorProcessed()
	{
	}
}
