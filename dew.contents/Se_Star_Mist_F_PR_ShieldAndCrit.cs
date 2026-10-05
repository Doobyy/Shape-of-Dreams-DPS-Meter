using System;
using Mirror;
using UnityEngine;

public class Se_Star_Mist_F_PR_ShieldAndCrit : StarEffect
{
	public float shieldRatio = 1f;

	public float shieldDecayDuration = 8f;

	public float shieldDecayDurationBonusPerLevel = 0.7f;

	public override Type heroType => typeof(Hero_Mist);

	public override Type skillType => typeof(St_R_Parry);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.ActorEvent_OnAbilityInstanceCreated += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
		}
	}

	private void ActorEventOnAbilityInstanceCreated(EventInfoAbilityInstance obj)
	{
		if (!(obj.instance is Se_R_Parry_End se_R_Parry_End))
		{
			return;
		}
		int num = ((!(se_R_Parry_End.firstTrigger is SkillTrigger skillTrigger)) ? 1 : skillTrigger.level);
		float num2 = se_R_Parry_End.blockedDamage * shieldRatio;
		float num3 = shieldDecayDuration + shieldDecayDurationBonusPerLevel * (float)(num - 1);
		if (se_R_Parry_End.blockedDamage > 1f)
		{
			Se_GenericShield_OneShot se_GenericShield_OneShot = victim.Status.FindStatusEffect((Se_GenericShield_OneShot s) => (UnityEngine.Object)(object)s.parentActor == (UnityEngine.Object)(object)this);
			if ((UnityEngine.Object)(object)se_GenericShield_OneShot != null)
			{
				se_GenericShield_OneShot.SetAmount(se_GenericShield_OneShot.shield.amount + num2);
				se_GenericShield_OneShot.SetTimer(num3);
			}
			else
			{
				GiveShield(victim, num2, num3, isDecay: true);
			}
		}
		RefValue<StatusEffect> basicEffect = new RefValue<StatusEffect>();
		basicEffect.value = CreateBasicEffect(victim, new AttackCriticalEffect
		{
			onUse = () =>
			{
				if (!basicEffect.value.IsNullOrInactive())
				{
					basicEffect.value.DestroyIfActive();
				}
			}
		}, 10f, "ParryCrit", DuplicateEffectBehavior.UsePrevious);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)victim != null)
		{
			victim.ActorEvent_OnAbilityInstanceCreated -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceCreated);
		}
	}

	private void MirrorProcessed()
	{
	}
}
