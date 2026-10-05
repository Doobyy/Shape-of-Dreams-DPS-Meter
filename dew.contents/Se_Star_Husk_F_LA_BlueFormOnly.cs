using System;
using Mirror;
using UnityEngine;

public class Se_Star_Husk_F_LA_BlueFormOnly : StarEffect
{
	public ChargingChannelData chargingData;

	public float addedRangeRatio = 0.2f;

	public float chargeTime = 1f;

	public float addedInvulnerableTimeRatio = 0.5f;

	public GameObject fxCast;

	public override Type heroType => typeof(Hero_Husk);

	public override Type skillType => typeof(St_Q_Laceration);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoSkill((SkillTrigger s) =>
		{
			if (s is St_Q_Laceration st_Q_Laceration)
			{
				st_Q_Laceration.SetLockBlueForm(locked: true);
			}
			chargingData.chargeFullDuration = chargeTime;
			return () =>
			{
				if (s is St_Q_Laceration st_Q_Laceration2)
				{
					st_Q_Laceration2.SetLockBlueForm(locked: false);
				}
			};
		});
		hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(OnBeforePrepare);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(OnBeforePrepare);
		}
	}

	private void OnBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (obj.instance is Ai_Q_Laceration_Circle ai_Q_Laceration_Circle)
		{
			ai_Q_Laceration_Circle.NetworkuseChargingMode = true;
			ai_Q_Laceration_Circle.addedInvulnerableTimeRatio = addedInvulnerableTimeRatio;
			ai_Q_Laceration_Circle.chargingData = (ChargingChannelData)chargingData.Clone();
			ai_Q_Laceration_Circle.chargingData.chargeFullDuration = chargeTime;
			ai_Q_Laceration_Circle.maxRangeMultiplier = 1f + addedRangeRatio;
			ai_Q_Laceration_Circle.startEffect = fxCast;
		}
	}

	private void MirrorProcessed()
	{
	}
}
