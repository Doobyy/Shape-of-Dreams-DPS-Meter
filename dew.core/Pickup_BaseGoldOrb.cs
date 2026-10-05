using System;
using Mirror;
using UnityEngine;

public class Pickup_BaseGoldOrb : PickupInstance
{
	[NonSerialized]
	public int amount;

	[NonSerialized]
	public Hero target;

	[NonSerialized]
	public bool isKillGold;

	[NonSerialized]
	public bool isGivenByOtherPlayer;

	private Action<EventInfoLoadRoom> _cachedDoPickupImmediately;

	public override bool reuseInRoom => true;

	public override bool isDestroyedOnRoomChange => false;

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
		GrantGold(amount, target, isKillGold, isGivenByOtherPlayer);
	}

	public static void GrantGold(int amount, Hero target, bool isKillGold, bool isGivenByOtherPlayer)
	{
		if ((UnityEngine.Object)(object)target != null)
		{
			if (!((UnityEngine.Object)(object)target.owner == null))
			{
				float num = amount;
				if (isKillGold)
				{
					num *= DewBuildProfile.current.killGoldMultiplier;
					num *= target.owner.monsterKillGoldMultiplier;
				}
				int num2 = DewMath.RandomRoundToInt(num);
				if (isGivenByOtherPlayer)
				{
					target.owner.AddGold(num2);
				}
				else
				{
					target.owner.EarnGold(num2);
				}
			}
			return;
		}
		float num3 = (float)amount / (float)DewPlayer.gamePlayers.Count;
		foreach (DewPlayer gamePlayer in DewPlayer.gamePlayers)
		{
			float num4 = num3;
			if (isKillGold)
			{
				num4 *= DewBuildProfile.current.killGoldMultiplier;
				num4 *= gamePlayer.monsterKillGoldMultiplier;
			}
			int num5 = DewMath.RandomRoundToInt(num4);
			if (isGivenByOtherPlayer)
			{
				gamePlayer.AddGold(num5);
			}
			else
			{
				gamePlayer.EarnGold(num5);
			}
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
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted -= new Action<EventInfoLoadRoom>(DoPickupImmediately);
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
