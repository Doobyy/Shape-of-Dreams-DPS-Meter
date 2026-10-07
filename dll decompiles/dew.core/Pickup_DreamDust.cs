using System;
using Mirror;
using UnityEngine;

public class Pickup_DreamDust : PickupInstance
{
	public SafeAction<DewPlayer, int> onGiveDreamDust;

	[NonSerialized]
	public int amount;

	[NonSerialized]
	public Hero target;

	[NonSerialized]
	public bool isGivenByOtherPlayer;

	private Action<EventInfoLoadRoom> _cachedDoPickupImmediately;

	public override bool reuseInRoom => true;

	public override bool isDestroyedOnRoomChange => false;

	public override void ClearPooledEventsAndProcessors()
	{
		base.ClearPooledEventsAndProcessors();
		onGiveDreamDust?.Clear();
	}

	protected override bool CanBeUsedBy(Hero hero)
	{
		if ((UnityEngine.Object)(object)target != null)
		{
			if (Time.time - creationTime > pickupDelay)
			{
				return (UnityEngine.Object)(object)hero == (UnityEngine.Object)(object)target;
			}
			return false;
		}
		return base.CanBeUsedBy(hero);
	}

	protected override void OnPickup(Hero hero)
	{
		base.OnPickup(hero);
		if ((UnityEngine.Object)(object)target != null)
		{
			if (!((UnityEngine.Object)(object)target.owner == null))
			{
				if (isGivenByOtherPlayer)
				{
					target.owner.AddDreamDust(amount);
				}
				else
				{
					target.owner.EarnDreamDust(amount);
				}
				onGiveDreamDust?.Invoke(target.owner, amount);
			}
			return;
		}
		float value = (float)amount / (float)DewPlayer.gamePlayers.Count;
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			int b = DewMath.RandomRoundToInt(value);
			if (isGivenByOtherPlayer)
			{
				gamePlayer.AddDreamDust(b);
			}
			else
			{
				gamePlayer.EarnDreamDust(b);
			}
			onGiveDreamDust?.Invoke(gamePlayer, b);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted += new Action<EventInfoLoadRoom>(DoPickupImmediately);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted -= _cachedDoPickupImmediately;
		}
	}

	private void DoPickupImmediately(EventInfoLoadRoom obj)
	{
		if (!target.IsNullOrInactive())
		{
			OnPickup(target);
			Destroy();
			return;
		}
		Hero hero = Dew.SelectRandomAliveHero();
		if ((UnityEngine.Object)(object)hero != null && CanBeUsedBy(hero))
		{
			OnPickup(hero);
			Destroy();
		}
		else
		{
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
