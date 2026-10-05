using System;
using Mirror;
using UnityEngine;

public class LucidDream_WILD : LucidDream
{
	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer)
		{
			NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd += new Action<Entity>(OnEntityAdd);
			NetworkedManagerBase<ZoneManager>.instance.onWorldGenerated += new Action(OnWorldGenerated);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
			OnWorldGenerated();
		}
	}

	private void ClientEventOnZoneLoaded(EventInfoLoadZone obj)
	{
		if (obj.isTraveling)
		{
			NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel = 0;
		}
	}

	private void OnWorldGenerated()
	{
		if (!(NetworkedManagerBase<ZoneManager>.instance.currentZone == null) && !NetworkedManagerBase<ZoneManager>.instance.currentZone.useSpecialGeneration && NetworkedManagerBase<ZoneManager>.instance.hunterSkippedTurns > 0)
		{
			NetworkedManagerBase<ZoneManager>.instance.hunterSkippedTurns = 0;
			NetworkedManagerBase<ZoneManager>.instance.AdvanceHunterTurn();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ActorManager>.instance != null)
			{
				NetworkedManagerBase<ActorManager>.instance.ClientEvent_OnEntityAdd -= new Action<Entity>(OnEntityAdd);
			}
			if ((UnityEngine.Object)(object)NetworkedManagerBase<ZoneManager>.instance != null)
			{
				NetworkedManagerBase<ZoneManager>.instance.onWorldGenerated -= new Action(OnWorldGenerated);
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded -= new Action<EventInfoLoadZone>(ClientEventOnZoneLoaded);
			}
		}
	}

	private void OnEntityAdd(Entity obj)
	{
		if (obj is Monster && !((UnityEngine.Object)(object)obj.owner != (UnityEngine.Object)(object)DewPlayer.creep) && !obj.IsAnyBoss() && !obj.Status.HasStatusEffect<Se_HunterBuff>())
		{
			CreateStatusEffect<Se_HunterBuff>(obj, new CastInfo(obj, obj)).enableGoldAndExpDrops = true;
			DewResources.GetByType<RoomMod_Hunted>(default(ResourceLoadSettings)).ApplyHunterStatBonusAndAIPrediction(obj, NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
		}
	}

	private void MirrorProcessed()
	{
	}
}
