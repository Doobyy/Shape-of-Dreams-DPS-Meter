using System;
using Mirror;
using UnityEngine;

public class Se_D_TheKillingFlow : StatusEffect
{
	public const int ProcessorPriority = 10;

	public ScalingValue atkSpdPercentageToAdRatio;

	public ScalingValue healAmountPerBossKill;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			victim.Status.finalStatsProcessors.Add(Processor, 10);
			victim.Control.ClientEvent_OnTeleport += new Action<Vector3, Vector3>(ClientEventOnTeleport);
			victim.Control.ClientEvent_OnDisplacementStarted += new Action<Displacement>(ClientEventOnDisplacementStarted);
			((Hero)victim).ClientHeroEvent_OnKillOrAssist += new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
			victim.Status.CalculateStats();
		}
	}

	private void ClientHeroEventOnKillOrAssist(EventInfoKill obj)
	{
		if (obj.victim.IsAnyBoss())
		{
			Heal(GetValue(healAmountPerBossKill)).Dispatch(victim);
		}
	}

	private void ClientEventOnDisplacementStarted(Displacement obj)
	{
		if (!NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && ManagerBase<TransitionManager>.instance.state != TransitionManager.StateType.Loading && obj.isFriendly)
		{
			ResetCooldown(victim.Ability.attackAbility);
			if (victim.Status.TryGetStatusEffect<Se_D_TheKillingFlow_Crit>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_D_TheKillingFlow_Crit>(victim);
			}
		}
	}

	private void ClientEventOnTeleport(Vector3 arg1, Vector3 arg2)
	{
		if (!NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && ManagerBase<TransitionManager>.instance.state != TransitionManager.StateType.Loading)
		{
			ResetCooldown(victim.Ability.attackAbility);
			if (victim.Status.TryGetStatusEffect<Se_D_TheKillingFlow_Crit>(out var effect))
			{
				effect.ResetTimer();
			}
			else
			{
				CreateStatusEffect<Se_D_TheKillingFlow_Crit>(victim);
			}
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if ((UnityEngine.Object)(object)victim != null)
		{
			victim.Status.finalStatsProcessors.Remove(Processor);
			victim.Control.ClientEvent_OnTeleport -= new Action<Vector3, Vector3>(ClientEventOnTeleport);
			victim.Control.ClientEvent_OnDisplacementStarted -= new Action<Displacement>(ClientEventOnDisplacementStarted);
			((Hero)victim).ClientHeroEvent_OnKillOrAssist -= new Action<EventInfoKill>(ClientHeroEventOnKillOrAssist);
			victim.Status.CalculateStats();
		}
	}

	private void Processor(ref FinalStats data)
	{
		if (data.attackSpeedMultiplier <= 1f)
		{
			((St_D_TheKillingFlow)firstTrigger).gainedAd = 0;
			return;
		}
		float num = (data.attackSpeedMultiplier - 1f) * 100f;
		data.attackSpeedMultiplier = 1f;
		float num2 = num * GetValue(atkSpdPercentageToAdRatio);
		data.attackDamage += num2 * (1f + victim.Status.bonusStats.attackDamagePercentage * 0.01f);
		((St_D_TheKillingFlow)firstTrigger).gainedAd = Mathf.RoundToInt(num2);
	}

	private void MirrorProcessed()
	{
	}
}
