using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Se_Star_Yubar_I_MoreDreamDustWithPenalty : StarEffect
{
	public StarScalingValue dreamDustAmp;

	public float maxHealthPenaltyPercentage;

	private readonly List<(Pickup_DreamDust dd, Action<DewPlayer, int> handler)> _dreamDustSubscriptions = new List<(Pickup_DreamDust, Action<DewPlayer, int>)>();

	public override Type heroType => typeof(Hero_Yubar);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd += new Action<Actor>(OnActorAdd);
			DoStatBonus(new StatBonus
			{
				maxHealthPercentage = 0f - maxHealthPenaltyPercentage
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnActorAdd -= new Action<Actor>(OnActorAdd);
		}
		foreach (var dreamDustSubscription in _dreamDustSubscriptions)
		{
			if ((UnityEngine.Object)(object)dreamDustSubscription.dd != null)
			{
				dreamDustSubscription.dd.onGiveDreamDust -= dreamDustSubscription.handler;
			}
		}
		_dreamDustSubscriptions.Clear();
	}

	private void OnActorAdd(Actor obj)
	{
		if (!(obj is Pickup_DreamDust { isGivenByOtherPlayer: false } pickup_DreamDust))
		{
			return;
		}
		Action<DewPlayer, int> action = (DewPlayer p, int amount) =>
		{
			if (!((UnityEngine.Object)(object)p != (UnityEngine.Object)(object)player))
			{
				int num = DewMath.RandomRoundToInt((float)amount * GetValue(dreamDustAmp));
				if (num > 0)
				{
					player.EarnDreamDust(num);
				}
			}
		};
		pickup_DreamDust.onGiveDreamDust += action;
		_dreamDustSubscriptions.Add((pickup_DreamDust, action));
	}

	private void MirrorProcessed()
	{
	}
}
