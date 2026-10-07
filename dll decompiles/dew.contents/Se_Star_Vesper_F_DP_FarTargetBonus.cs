using System;
using Mirror;
using UnityEngine;

public class Se_Star_Vesper_F_DP_FarTargetBonus : StarEffect
{
	public float healthPenaltyPercentage = 15f;

	public float rangeAmp = 0.3f;

	public float maxDamageAmp = 1f;

	public float maxRadiusAmp = 0.5f;

	public override Type heroType => typeof(Hero_Vesper);

	public override Type skillType => typeof(St_Q_Discipline);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)skill != null)
			{
				skill.configs[0].castMethod._range *= 1f + rangeAmp;
				skill.SyncCastMethodChanges(0);
			}
			victim.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			DoStatBonus(new StatBonus
			{
				maxHealthPercentage = 0f - healthPenaltyPercentage
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)skill != null)
			{
				skill.configs[0].castMethod._range /= 1f + rangeAmp;
				skill.SyncCastMethodChanges(0);
			}
			if ((UnityEngine.Object)(object)victim != null)
			{
				victim.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
			}
		}
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		if (!(obj.instance is Se_Q_Discipline_Jump se_Q_Discipline_Jump))
		{
			return;
		}
		Vector3 v = (((UnityEngine.Object)(object)se_Q_Discipline_Jump.info.target != null) ? se_Q_Discipline_Jump.info.target.agentPosition : se_Q_Discipline_Jump.info.point);
		float num = 3f;
		float num2 = 8.5f;
		float num3 = Vector2.Distance(v.ToXY(), victim.agentPosition.ToXY());
		float normalizedStrength = Mathf.Clamp01((num3 - num) / (num2 - num));
		se_Q_Discipline_Jump.ActorEvent_OnAbilityInstanceBeforePrepare += (Action<EventInfoAbilityInstance>)((EventInfoAbilityInstance eventInfoAbilityInstance) =>
		{
			if (eventInfoAbilityInstance.instance is Ai_Q_Discipline_Stomp ai_Q_Discipline_Stomp)
			{
				ai_Q_Discipline_Stomp.damageMultiplier *= 1f + normalizedStrength * maxDamageAmp;
				ai_Q_Discipline_Stomp.NetworkradiusMultiplier = ai_Q_Discipline_Stomp.radiusMultiplier * (1f + normalizedStrength * maxRadiusAmp);
			}
		});
	}

	private void MirrorProcessed()
	{
	}
}
