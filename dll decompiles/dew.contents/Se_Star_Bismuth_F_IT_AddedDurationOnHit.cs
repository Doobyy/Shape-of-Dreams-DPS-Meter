using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Bismuth_F_IT_AddedDurationOnHit : StarEffect
{
	public float cooldownPenalty = 1f;

	public float addedDurationPerHit = 0.2f;

	public float maxAddedDuration = 2f;

	private readonly List<(Ai_QR_InfernalTales ai, Action<Entity> handler)> _onHitSubscriptions = new List<(Ai_QR_InfernalTales, Action<Entity>)>();

	public override Type heroType => typeof(Hero_Bismuth);

	public override Type skillType => typeof(St_QR_InfernalTales);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			DoSkillBonusAll(new SkillBonus
			{
				cooldownOffset = cooldownPenalty
			});
			hero.ActorEvent_OnAbilityInstanceBeforePrepare += new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)hero != null)
		{
			hero.ActorEvent_OnAbilityInstanceBeforePrepare -= new Action<EventInfoAbilityInstance>(ActorEventOnAbilityInstanceBeforePrepare);
		}
		foreach (var onHitSubscription in _onHitSubscriptions)
		{
			if ((UnityEngine.Object)(object)onHitSubscription.ai != null)
			{
				onHitSubscription.ai.onHit -= onHitSubscription.handler;
			}
		}
		_onHitSubscriptions.Clear();
	}

	private void ActorEventOnAbilityInstanceBeforePrepare(EventInfoAbilityInstance obj)
	{
		AbilityInstance instance = obj.instance;
		Ai_QR_InfernalTales ai = instance as Ai_QR_InfernalTales;
		if (ai == null)
		{
			return;
		}
		RefValue<float> totalAdded = new RefValue<float>(0f);
		Action<Entity> action = (Entity _) =>
		{
			int num = DewMath.RandomRoundToInt(Mathf.Min(addedDurationPerHit, maxAddedDuration - (float)totalAdded) / ai.tickInterval);
			if (num > 0)
			{
				ai.ticks += num;
				totalAdded.value += (float)num * ai.tickInterval;
			}
		};
		ai.onHit += action;
		_onHitSubscriptions.Add((ai, action));
	}

	private void MirrorProcessed()
	{
	}
}
